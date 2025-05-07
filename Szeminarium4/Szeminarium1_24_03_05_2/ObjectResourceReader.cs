using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Szeminarium1_24_03_05_2
{
    internal class ObjectResourceReader
    {
        public static unsafe GlObject CreateObjectFromResource(GL Gl, string resourceName)
        {
            List<float[]> objVertices = new List<float[]>();
            List<float[]> objNormals = new List<float[]>();     // normalvektorok
            List<(int vIdx, int? vnIdx)> faceVertexData = new List<(int, int?)>();
            List<int[]> faceIndices = new List<int[]>();
            bool usesVertexNormals = false;     // jelzi, hogy van e normalvektor hasznalva

            string fullResourceName = "Szeminarium1_24_03_05_2.Resources." + resourceName;
            using (var objStream = typeof(ObjectResourceReader).Assembly.GetManifestResourceStream(fullResourceName))
            using (var objReader = new StreamReader(objStream))
            {
                while (!objReader.EndOfStream)
                {
                    var line = objReader.ReadLine();

                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))        // ures sorokat es kommenteket kihagy
                        continue;

                    var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);      // feldarabolja a sort szokozok menten
                    if (parts.Length == 0)
                        continue;

                    switch (parts[0])
                    {
                        case "v":       // a v kezdetu sor koordinatat jelent, beolvassa a 3 koordinatat
                            objVertices.Add(new float[]
                            {
                                float.Parse(parts[1], CultureInfo.InvariantCulture),
                                float.Parse(parts[2], CultureInfo.InvariantCulture),
                                float.Parse(parts[3], CultureInfo.InvariantCulture)
                            });
                            break;

                        case "vn":      // a vn a normalvektor, szinten 3 komponense van
                            objNormals.Add(new float[]
                            {
                                float.Parse(parts[1], CultureInfo.InvariantCulture),
                                float.Parse(parts[2], CultureInfo.InvariantCulture),
                                float.Parse(parts[3], CultureInfo.InvariantCulture)
                            });
                            break;

                        case "f":       // a haromszog minden csucsahoz elmenti a pontot es a normalvektor indexet(ha van) csokkentve 1-el
                            int[] face = new int[3];
                            for (int i = 0; i < 3; i++)
                            {
                                var vertexParts = parts[i + 1].Split('/');
                                int vIdx = int.Parse(vertexParts[0]) - 1;
                                int? vnIdx = (vertexParts.Length >= 3 && !string.IsNullOrEmpty(vertexParts[2]))
                                    ? int.Parse(vertexParts[2]) - 1
                                    : null;

                                faceVertexData.Add((vIdx, vnIdx));
                                face[i] = vIdx;

                                if (vnIdx.HasValue)
                                    usesVertexNormals = true;
                            }
                            faceIndices.Add(face);
                            break;
                    }
                }
            }


            // lista minden vertexbol sajat strukturaval, ami tartalmazza a poziciot es a normalt
            List<ObjVertexTransformationData> vertexTransformations = new List<ObjVertexTransformationData>();
            for (int i = 0; i < objVertices.Count; ++i)
            {
                var coord = objVertices[i];
                Vector3D<float> normal = Vector3D<float>.Zero;

                if (usesVertexNormals)
                {
                    var match = faceVertexData.FirstOrDefault(p => p.vIdx == i && p.vnIdx.HasValue);
                    if (match.vnIdx.HasValue && match.vnIdx.Value < objNormals.Count)
                    {
                        var n = objNormals[match.vnIdx.Value];
                        normal = new Vector3D<float>(n[0], n[1], n[2]);
                    }
                }

                vertexTransformations.Add(new ObjVertexTransformationData(
                    new Vector3D<float>(coord[0], coord[1], coord[2]),
                    normal,
                    1
                ));
            }

            
            // ha nem volt normalvektor, akkor kiszamolja a haromszogekbol az erteket vektorialis szorzattal
            if (!usesVertexNormals)
            {
                foreach (var face in faceIndices)
                {
                    var a = vertexTransformations[face[0]];
                    var b = vertexTransformations[face[1]];
                    var c = vertexTransformations[face[2]];

                    var normal = Vector3D.Normalize(Vector3D.Cross(b.Coordinates - a.Coordinates, c.Coordinates - a.Coordinates));

                    a.UpdateNormalWithContributionFromAFace(normal);
                    b.UpdateNormalWithContributionFromAFace(normal);
                    c.UpdateNormalWithContributionFromAFace(normal);
                }
            }

            
            List<float> glVertices = new List<float>();
            List<float> glColors = new List<float>();
            foreach (var vt in vertexTransformations)
            {
                glVertices.Add(vt.Coordinates.X);
                glVertices.Add(vt.Coordinates.Y);
                glVertices.Add(vt.Coordinates.Z);

                glVertices.Add(vt.Normal.X);
                glVertices.Add(vt.Normal.Y);
                glVertices.Add(vt.Normal.Z);

                glColors.AddRange([1.0f, 0.0f, 0.0f, 1.0f]);
            }

            List<uint> glIndexArray = new List<uint>();
            foreach (var face in faceIndices)
            {
                glIndexArray.Add((uint)face[0]);
                glIndexArray.Add((uint)face[1]);
                glIndexArray.Add((uint)face[2]);
            }

            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            uint offsetPos = 0;
            uint offsetNormals = offsetPos + 3 * sizeof(float);
            uint vertexSize = offsetNormals + 3 * sizeof(float);

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, vertices);
            Gl.BufferData(BufferTargetARB.ArrayBuffer, (ReadOnlySpan<float>)glVertices.ToArray().AsSpan(), BufferUsageARB.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);
            Gl.EnableVertexAttribArray(0);
            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, true, vertexSize, (void*)offsetNormals);
            Gl.EnableVertexAttribArray(2);

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, colors);
            Gl.BufferData(BufferTargetARB.ArrayBuffer, (ReadOnlySpan<float>)glColors.ToArray().AsSpan(), BufferUsageARB.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, indices);
            Gl.BufferData(BufferTargetARB.ElementArrayBuffer, (ReadOnlySpan<uint>)glIndexArray.ToArray().AsSpan(), BufferUsageARB.StaticDraw);

            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            Gl.BindVertexArray(0);

            return new GlObject(vao, vertices, colors, indices, (uint)glIndexArray.Count, Gl);
        }
    }
}
