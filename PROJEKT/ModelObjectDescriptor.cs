using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;
using StbImageSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Szeminarium;

namespace GrafikaSzeminarium
{
    internal class ModelObjectDescriptor : IDisposable
    {
        private bool disposedValue;

        public uint Vao { get; private set; }
        public uint Vertices { get; private set; }
        public uint Colors { get; private set; }
        public uint? Texture { get; private set; } = new uint?();
        public uint Indices { get; private set; }
        public uint IndexArrayLength { get; private set; }

        private GL Gl;

        public unsafe static ModelObjectDescriptor CreateSkyBox(GL Gl)
        {
            // counter clockwise is front facing
            // vx, vy, vz, nx, ny, nz, tu, tv
            float[] vertexArray = new float[] {
                // top face
                -0.5f, 0.5f, 0.5f, 0f, -1f, 0f, 1f/4f, 0f/3f,
                0.5f, 0.5f, 0.5f, 0f, -1f, 0f, 2f/4f, 0f/3f,
                0.5f, 0.5f, -0.5f, 0f, -1f, 0f, 2f/4f, 1f/3f,
                -0.5f, 0.5f, -0.5f, 0f, -1f, 0f, 1f/4f, 1f/3f,

                // front face
                -0.5f, 0.5f, 0.5f, 0f, 0f, -1f, 1, 1f/3f,
                -0.5f, -0.5f, 0.5f, 0f, 0f, -1f, 4f/4f, 2f/3f,
                0.5f, -0.5f, 0.5f, 0f, 0f, -1f, 3f/4f, 2f/3f,
                0.5f, 0.5f, 0.5f, 0f, 0f, -1f,  3f/4f, 1f/3f,

                // left face
                -0.5f, 0.5f, 0.5f, 1f, 0f, 0f, 0, 1f/3f,
                -0.5f, 0.5f, -0.5f, 1f, 0f, 0f,1f/4f, 1f/3f,
                -0.5f, -0.5f, -0.5f, 1f, 0f, 0f, 1f/4f, 2f/3f,
                -0.5f, -0.5f, 0.5f, 1f, 0f, 0f, 0f/4f, 2f/3f,

                // bottom face
                -0.5f, -0.5f, 0.5f, 0f, 1f, 0f, 1f/4f, 1f,
                0.5f, -0.5f, 0.5f,0f, 1f, 0f, 2f/4f, 1f,
                0.5f, -0.5f, -0.5f,0f, 1f, 0f, 2f/4f, 2f/3f,
                -0.5f, -0.5f, -0.5f,0f, 1f, 0f, 1f/4f, 2f/3f,

                // back face
                0.5f, 0.5f, -0.5f, 0f, 0f, 1f, 2f/4f, 1f/3f,
                -0.5f, 0.5f, -0.5f, 0f, 0f, 1f, 1f/4f, 1f/3f,
                -0.5f, -0.5f, -0.5f,0f, 0f, 1f, 1f/4f, 2f/3f,
                0.5f, -0.5f, -0.5f,0f, 0f, 1f, 2f/4f, 2f/3f,

                // right face
                0.5f, 0.5f, 0.5f, -1f, 0f, 0f, 3f/4f, 1f/3f,
                0.5f, 0.5f, -0.5f,-1f, 0f, 0f, 2f/4f, 1f/3f,
                0.5f, -0.5f, -0.5f, -1f, 0f, 0f, 2f/4f, 2f/3f,
                0.5f, -0.5f, 0.5f, -1f, 0f, 0f, 3f/4f, 2f/3f,
            };

            float[] colorArray = new float[] {
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,

                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,

                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,

                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,

                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,

                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
            };

            uint[] indexArray = new uint[] {
                0, 2, 1,
                0, 3, 2,

                4, 6, 5,
                4, 7, 6,

                8, 10, 9,
                10, 8, 11,

                12, 13, 14,
                12, 14, 15,

                17, 19, 16,
                17, 18, 19,

                20, 21, 22,
                20, 22, 23
            };

            var skyboxImage = ReadTextureImage("skybox.png");

            return CreateObjectDescriptorFromArrays(Gl, vertexArray, colorArray, indexArray, skyboxImage);
        }

        private static unsafe ModelObjectDescriptor CreateObjectDescriptorFromArrays(GL Gl, float[] vertexArray, float[] colorArray, uint[] indexArray,
            ImageResult textureImage = null)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);
            // 0 is position
            // 2 is normals
            // 3 is texture
            uint offsetPos = 0;
            uint offsetNormals = offsetPos + 3 * sizeof(float);
            uint offsetTexture = offsetNormals + 3 * sizeof(float);
            uint vertexSize = offsetTexture + 2 * sizeof(float);

            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);
            Gl.EnableVertexAttribArray(0);
            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, true, vertexSize, (void*)offsetNormals);
            Gl.EnableVertexAttribArray(2);
            Gl.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetTexture);
            Gl.EnableVertexAttribArray(3);
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);


            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);
            // 1 is color
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);

            uint? texture = new uint?();

            if (textureImage != null)
            {
                texture = Gl.GenTexture();
                Gl.ActiveTexture(TextureUnit.Texture0);
                // bind texture
                Gl.BindTexture(TextureTarget.Texture2D, texture.Value);

                Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)textureImage.Width,
                    (uint)textureImage.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (ReadOnlySpan<byte>)textureImage.Data.AsSpan());
                Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

                Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                // unbinde texture
                Gl.BindTexture(TextureTarget.Texture2D, 0);
            }

            return new ModelObjectDescriptor() { Vao = vao, Vertices = vertices, Colors = colors, Indices = indices, IndexArrayLength = (uint)indexArray.Length, Gl = Gl, Texture = texture };
        }

        private static unsafe ImageResult ReadTextureImage(string textureResource)
        {
            ImageResult result;
            using (Stream skyeboxStream
                = typeof(ModelObjectDescriptor).Assembly.GetManifestResourceStream("GrafikaSzeminarium.Resources." + textureResource))
                result = ImageResult.FromStream(skyeboxStream, ColorComponents.RedGreenBlueAlpha);

            return result;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null


                // always unbound the vertex buffer first, so no halfway results are displayed by accident
                Gl.DeleteBuffer(Vertices);
                Gl.DeleteBuffer(Colors);
                Gl.DeleteBuffer(Indices);
                Gl.DeleteVertexArray(Vao);

                disposedValue = true;
            }
        }

        ~ModelObjectDescriptor()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        internal static ModelObjectDescriptor CreateTexturedObj(GL Gl)
        {
            List<float[]> objVertices = new List<float[]>();
            List<float[]> objNormals = new List<float[]>();
            List<float[]> objTexcoords = new List<float[]>();
            List<(int v, int vt, int vn)[]> objFaces = new List<(int, int, int)[]>();

            string materialFileName = null;
            string materialName = null;
            string textureFileName = null;

            string fullResourceName = "GrafikaSzeminarium.Resources.hazmat_fixed.obj";
            using (var objStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName))
            using (var objReader = new StreamReader(objStream))
            {
                while (!objReader.EndOfStream)
                {
                    var line = objReader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line) || !line.Contains(' '))
                        continue;

                    var lineClassifier = line.Substring(0, line.IndexOf(' '));
                    var lineData = line.Substring(line.IndexOf(" ")).Trim().Split(' ');

                    switch (lineClassifier)
                    {
                        case "mtllib":
                            materialFileName = lineData[0]; // hazmat.mtl
                            break;
                        case "usemtl":
                            materialName = lineData[0]; // Material.001
                            break;
                        case "v":
                            float[] vertex = new float[3];
                            for (int i = 0; i < 3; i++)
                                vertex[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                            objVertices.Add(vertex);
                            break;
                        case "vn":
                            float[] normal = new float[3];
                            for (int i = 0; i < 3; i++)
                                normal[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                            objNormals.Add(normal);
                            break;
                        case "vt":
                            float[] texcoord = new float[2];
                            for (int i = 0; i < 2; i++)
                                texcoord[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                            objTexcoords.Add(texcoord);
                            break;
                        case "f":
                            var face = new (int, int, int)[3];
                            for (int i = 0; i < 3; i++)
                            {
                                var parts = lineData[i].Split('/');
                                int vertexIndex = int.Parse(parts[0]) - 1;
                                int texcoordIndex = parts.Length > 1 && !string.IsNullOrEmpty(parts[1]) ? int.Parse(parts[1]) - 1 : -1;
                                int normalIndex = parts.Length > 2 && !string.IsNullOrEmpty(parts[2]) ? int.Parse(parts[2]) - 1 : -1;
                                face[i] = (vertexIndex, texcoordIndex, normalIndex);
                            }
                            objFaces.Add(face);
                            break;
                        default:
                            break;
                    }
                }
            }

            if (materialFileName != null)
            {
                string mtlResourceName = "GrafikaSzeminarium.Resources." + materialFileName;
                using var mtlStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(mtlResourceName);
                using var mtlReader = new StreamReader(mtlStream);

                bool correctMaterial = materialName == null;

                while (!mtlReader.EndOfStream)
                {
                    string line = mtlReader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line) || !line.Contains(' '))
                        continue;

                    var parts = line.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2)
                        continue;

                    if (parts[0] == "newmtl")
                    {
                        correctMaterial = (parts[1] == materialName);
                    }
                    else if (parts[0] == "map_Kd" && correctMaterial)
                    {
                        textureFileName = parts[1];         // mindig frissitem, ha tobb van
                    }

                }
            }


            List<float> glVertices = new List<float>();
            List<float> glTexcoords = new List<float>();
            List<float> glColors = new List<float>();
            List<uint> glIndexArray = new List<uint>();

            foreach (var face in objFaces)
            {
                for (int i = 0; i < 3; i++)
                {
                    var (vIdx, vtIdx, vnIdx) = face[i];

                    var v = objVertices[vIdx];
                    var n = vnIdx >= 0 ? objNormals[vnIdx] : new float[] { 0, 0, 0 };
                    var t = vtIdx >= 0 ? objTexcoords[vtIdx] : new float[] { 0, 0 };

                    t[1] = 1.0f - t[1];         // Flip Y koordinata a Blenderhez

                    glVertices.AddRange(v);         // position
                    glVertices.AddRange(n);         // normal
                    glVertices.AddRange(t);         // texture

                    glColors.AddRange(new float[] { 1f, 1f, 1f, 1f });

                    glIndexArray.Add((uint)glIndexArray.Count);
                }
            }

            var textureImage = ReadTextureImage(textureFileName ?? "texture.png");

            return CreateObjectDescriptorFromArrays(Gl,
                glVertices.ToArray(),
                glColors.ToArray(),
                glIndexArray.ToArray(),
                textureImage);
        }


        internal static ModelObjectDescriptor CreateTreeObject(GL Gl)
        {
            List<float[]> objVertices = new List<float[]>();
            List<float[]> objNormals = new List<float[]>();
            List<float[]> objTexcoords = new List<float[]>();
            List<(int v, int vt, int vn)[]> objFaces = new List<(int, int, int)[]>();

            string materialFileName = null;
            string materialName = null;
            string textureFileName = null;

            string fullResourceName = "GrafikaSzeminarium.Resources.Tree1.obj";
            using (var objStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName))
            using (var objReader = new StreamReader(objStream))
            {
                while (!objReader.EndOfStream)
                {
                    var line = objReader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line) || !line.Contains(' '))
                        continue;

                    var lineClassifier = line.Substring(0, line.IndexOf(' '));
                    var lineData = line.Substring(line.IndexOf(" ")).Trim().Split(' ');

                    switch (lineClassifier)
                    {
                        case "mtllib":
                            materialFileName = lineData[0];
                            break;
                        case "usemtl":
                            materialName = lineData[0];
                            break;
                        case "v":
                            objVertices.Add(lineData.Take(3).Select(s => float.Parse(s, CultureInfo.InvariantCulture)).ToArray());
                            break;
                        case "vn":
                            objNormals.Add(lineData.Take(3).Select(s => float.Parse(s, CultureInfo.InvariantCulture)).ToArray());
                            break;
                        case "vt":
                            objTexcoords.Add(lineData.Take(2).Select(s => float.Parse(s, CultureInfo.InvariantCulture)).ToArray());
                            break;
                        case "f":
                            var face = new (int, int, int)[3];
                            for (int i = 0; i < 3; i++)
                            {
                                var parts = lineData[i].Split('/');
                                int v = int.Parse(parts[0]) - 1;
                                int vt = parts.Length > 1 && parts[1] != "" ? int.Parse(parts[1]) - 1 : -1;
                                int vn = parts.Length > 2 && parts[2] != "" ? int.Parse(parts[2]) - 1 : -1;
                                face[i] = (v, vt, vn);
                            }
                            objFaces.Add(face);
                            break;
                    }
                }
            }

            if (materialFileName != null)
            {
                string mtlResourceName = "GrafikaSzeminarium.Resources." + materialFileName;
                using var mtlStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(mtlResourceName);
                using var mtlReader = new StreamReader(mtlStream);

                bool correctMaterial = materialName == null;

                while (!mtlReader.EndOfStream)
                {
                    string line = mtlReader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line) || !line.Contains(' '))
                        continue;

                    var parts = line.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2) continue;

                    if (parts[0] == "newmtl")
                        correctMaterial = (parts[1] == materialName);
                    else if (parts[0] == "map_Kd" && correctMaterial)
                    {
                        textureFileName = parts[1];
                        break;
                    }
                }
            }

            List<float> glVertices = new List<float>();
            List<float> glColors = new List<float>();
            List<uint> glIndices = new List<uint>();

            foreach (var face in objFaces)
            {
                for (int i = 0; i < 3; i++)
                {
                    var (vIdx, vtIdx, vnIdx) = face[i];
                    var v = objVertices[vIdx];
                    var n = vnIdx >= 0 ? objNormals[vnIdx] : new float[] { 0f, 0f, 0f };
                    var t = vtIdx >= 0 ? objTexcoords[vtIdx] : new float[] { 0f, 0f };

                    glVertices.AddRange(v);    // position
                    glVertices.AddRange(n);    // normal
                    glVertices.AddRange(t);    // texcoord

                    glColors.AddRange(new float[] { 1f, 1f, 1f, 1f });
                    glIndices.Add((uint)glIndices.Count);
                }
            }

            var textureImage = ReadTextureImage(textureFileName ?? "10447_Pine_Tree_v1_Diffuse.jpg");
            return CreateObjectDescriptorFromArrays(Gl, glVertices.ToArray(), glColors.ToArray(), glIndices.ToArray(), textureImage);
        }


        public static ModelObjectDescriptor CreateGroundPlane(GL Gl, string textureName = "grass1.jpg")
        {
            float size = 100000f;       // placc meret (fel oldalhossz)
            float repeat = 1000f;       // hanyszor ismetlodjon a textura

            float[] vertexArray = {
                -size, 0f, -size,  0f, 1f, 0f, 0f, 0f,
                 size, 0f, -size,  0f, 1f, 0f, repeat, 0f,
                 size, 0f,  size,  0f, 1f, 0f, repeat, repeat,
                -size, 0f,  size,  0f, 1f, 0f, 0f, repeat,
            };

            float[] colorArray = {
                1f, 1f, 1f, 1f,
                1f, 1f, 1f, 1f,
                1f, 1f, 1f, 1f,
                1f, 1f, 1f, 1f,
            };

            uint[] indexArray = {
                0, 1, 2,
                0, 2, 3
            };

            var groundTexture = ReadTextureImage(textureName);
            var desc = CreateObjectDescriptorFromArrays(Gl, vertexArray, colorArray, indexArray, groundTexture);

            // csak itt a placcnal kell a repeat
            if (desc.Texture.HasValue)
            {
                Gl.BindTexture(TextureTarget.Texture2D, desc.Texture.Value);
                Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.Repeat);
                Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.Repeat);
                Gl.BindTexture(TextureTarget.Texture2D, 0);
            }

            return desc;
        }
    }
}