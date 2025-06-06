﻿using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using System.Numerics;
using System.Reflection;
using Szeminarium;

namespace GrafikaSzeminarium
{
    internal class Program
    {
        private static IWindow graphicWindow;

        private static GL Gl;

        private static ImGuiController imguiController;

        private static ModelObjectDescriptor model;

        private static ModelObjectDescriptor skybox;

        private static ModelObjectDescriptor ground;

        private static ModelObjectDescriptor tree;

        private static CameraDescriptor camera = new CameraDescriptor();

        private static Vector3 modelPosition;
        private static Vector3D<float> objectPosition;
        private static float objectRotation = 0f;

        private static List<Vector3> treePositions = new List<Vector3>();

        private const float GroundY = -10f;

        private static bool isTurningLeft = false;
        private static bool isTurningRight = false;
        private static bool moveForward = false;
        private static bool moveBackward = false;
        private static bool moveLeft = false;
        private static bool moveRight = false;

        private static int health = 100;
        private const int MaxHealth = 100;

        private static double lastDamageTime = 0;
        private const double DamageCooldown = 0.5;

        private const string ModelMatrixVariableName = "uModel";
        private const string NormalMatrixVariableName = "uNormal";
        private const string ViewMatrixVariableName = "uView";
        private const string ProjectionMatrixVariableName = "uProjection";

        private const string LightColorVariableName = "uLightColor";
        private const string ViewPositionVariableName = "uViewPos";

        private const string ShinenessVariableName = "uShininess";

        private const string TextureVariableName = "uTexture";

        private static float shininess = 50;

        private static uint program;

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "PROJEKT";
            windowOptions.Size = new Silk.NET.Maths.Vector2D<int>(500, 500);

            graphicWindow = Window.Create(windowOptions);

            graphicWindow.Load += GraphicWindow_Load;
            graphicWindow.Update += GraphicWindow_Update;
            graphicWindow.Render += GraphicWindow_Render;
            graphicWindow.Closing += GraphicWindow_Closing;

            graphicWindow.Run();
        }

        private static void GraphicWindow_Closing()
        {
            model.Dispose();
            skybox.Dispose();
            tree.Dispose();
            imguiController.Dispose();
            Gl.DeleteProgram(program);
        }

        private static void GraphicWindow_Load()
        {
            Gl = graphicWindow.CreateOpenGL();

            var inputContext = graphicWindow.CreateInput();
            
            foreach (var keyboard in inputContext.Keyboards)
            {
                keyboard.KeyDown += (k, key, code) =>
                {
                    switch (key)
                    {
                        case Key.W:
                            moveForward = true; 
                            break;
                        case Key.S:
                            moveBackward = true; 
                            break;
                        case Key.A: 
                            moveLeft = true; 
                            break;
                        case Key.D: 
                            moveRight = true; 
                            break;
                        case Key.Left: 
                            isTurningLeft = true;
                            break;
                        case Key.Right: 
                            isTurningRight = true; 
                            break;
                    }
                };

                keyboard.KeyUp += (k, key, code) =>
                {
                    switch (key)
                    {
                        case Key.W: 
                            moveForward = false; 
                            break;
                        case Key.S: 
                            moveBackward = false;
                            break;
                        case Key.A: 
                            moveLeft = false;
                            break;
                        case Key.D:
                            moveRight = false; 
                            break;
                        case Key.Left: 
                            isTurningLeft = false;
                            break;
                        case Key.Right: 
                            isTurningRight = false;
                            break;
                    }
                };
            }

            graphicWindow.FramebufferResize += s =>
            {
                Gl.Viewport(s);
            };

            model = ModelObjectDescriptor.CreateTexturedObj(Gl);

            skybox = ModelObjectDescriptor.CreateSkyBox(Gl);

            tree = ModelObjectDescriptor.CreateTreeObject(Gl);

            ground = ModelObjectDescriptor.CreateGroundPlane(Gl, "grass1.jpg");

            Gl.ClearColor(System.Drawing.Color.White);

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);


            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, GetEmbeddedResourceAsString("Shaders.VertexShader.vert"));
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, GetEmbeddedResourceAsString("Shaders.FragmentShader.frag"));
            Gl.CompileShader(fshader);
            Gl.GetShader(fshader, ShaderParameterName.CompileStatus, out int fStatus);
            if (fStatus != (int)GLEnum.True)
                throw new Exception("Fragment shader failed to compile: " + Gl.GetShaderInfoLog(fshader));

            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);

            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);
            if ((ErrorCode)Gl.GetError() != ErrorCode.NoError)
            {

            }

            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }

            Random rng = new Random();

            for (int i = 0; i < 1000; i++)
            {
                float x = rng.Next(-15000, 15000);
                float z = rng.Next(-15000, 15000);
                treePositions.Add(new Vector3(x, GroundY, z));
            }

            imguiController = new ImGuiController(Gl, graphicWindow, inputContext);
        }

        private static string GetEmbeddedResourceAsString(string resourceRelativePath)
        {
            string resourceFullPath = Assembly.GetExecutingAssembly().GetName().Name + "." + resourceRelativePath;

            using (var resStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceFullPath))
            using (var resStreamReader = new StreamReader(resStream))
            {
                var text = resStreamReader.ReadToEnd();
                return text;
            }
        }

        private static void BindTexture(uint? texture)
        {
            if (texture.HasValue)
            {
                int location = Gl.GetUniformLocation(program, TextureVariableName);
                if (location == -1)
                    throw new Exception($"{TextureVariableName} uniform not found on shader.");

                Gl.Uniform1(location, 0);
                Gl.ActiveTexture(TextureUnit.Texture0);
                Gl.BindTexture(TextureTarget.Texture2D, texture.Value);
            }
        }

        private static void GraphicWindow_Update(double deltaTime)
        {
            if (health <= 0)
                return;

            imguiController.Update((float)deltaTime);

            float turnSpeed = 2.5f;
            float speed = 10f;

            objectPosition.Y = GroundY;     // a jatekos a talaj szintjen

            Vector3D<float> camPos = camera.Position;
            Vector3D<float> camTarget = camera.Target;
            Vector3D<float> camUp = camera.UpVector;

            Vector3D<float> forward = Vector3D.Normalize(camTarget - camPos);
            Vector3D<float> right = Vector3D.Normalize(Vector3D.Cross(forward, camUp));
            forward.Y = 0;
            right.Y = 0;
            forward = Vector3D.Normalize(forward);
            right = Vector3D.Normalize(right);

            if (isTurningLeft)
                objectRotation += turnSpeed * (float)deltaTime;
            if (isTurningRight)
                objectRotation -= turnSpeed * (float)deltaTime;
            if (moveForward)
                objectPosition += forward * speed;
            if (moveBackward)
                objectPosition -= forward * speed;
            if (moveLeft)
                objectPosition -= right * speed;
            if (moveRight)
                objectPosition += right * speed;

            Vector3D<float> cameraTargetPosition = objectPosition;      // kamera kovetes frissitese
            cameraTargetPosition.Y += 100f;

            if (camera.Mode == CameraDescriptor.CameraMode.BehindObject)        // ha 3. szemelyben
            {
                float scale = 20f;
                float camDistance = 4.0f * scale;
                float camHeight = 2.5f * scale;

                camera.UpdateFollowingBehind(objectPosition, objectRotation, distance: camDistance, height: camHeight);
            }
            else        // 1.szemelyben
            {
                float scale = 20f;
                camera.UpdateCamera(objectPosition, objectRotation, scale);
            }

            modelPosition = new Vector3(objectPosition.X, GroundY, objectPosition.Z);       // model poziciojat frissiti

            CheckCollisionWithTrees(graphicWindow.Time);
        }

        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit);
            Gl.Clear(ClearBufferMask.DepthBufferBit);

            Gl.UseProgram(program);

            SetUniform3(LightColorVariableName, new Vector3(1f, 1f, 1f));
            SetUniform3(ViewPositionVariableName, new Vector3(camera.Position.X, camera.Position.Y, camera.Position.Z));
            SetUniform1(ShinenessVariableName, shininess);

            var viewMatrix = Matrix4X4.CreateLookAt(camera.Position, camera.Target, camera.UpVector);
            SetMatrix(viewMatrix, ViewMatrixVariableName);

            var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>((float)(Math.PI / 2), 1024f / 768f, 0.1f, 1000000000f);
            SetMatrix(projectionMatrix, ProjectionMatrixVariableName);

            DrawSkyBox();

            var groundMatrix = Matrix4X4.CreateTranslation(0f, GroundY, 0f);
            SetModelMatrix(groundMatrix);
            BindTexture(ground.Texture);

            DrawModelObject(ground);


            var scale = 20f;

            var modelMatrix = Matrix4X4.CreateScale(scale) * Matrix4X4.CreateRotationY(objectRotation) * Matrix4X4.CreateTranslation(modelPosition.X, GroundY, modelPosition.Z);
            SetModelMatrix(modelMatrix);
            BindTexture(model.Texture);
            DrawModelObject(model);

            foreach (var pos in treePositions)
            {
                var treeMatrix = Matrix4X4.CreateScale(0.5f) * Matrix4X4.CreateTranslation(pos.X, pos.Y, pos.Z);
                SetModelMatrix(treeMatrix);
                BindTexture(tree.Texture);

                DrawModelObject(tree);
            }

            CameraDescriptor.CameraMode selectedMode = camera.Mode;

            ImGui.SetNextWindowPos(new Vector2(10, 10), ImGuiCond.Always);
            ImGui.Begin("Camera mode", ImGuiWindowFlags.AlwaysAutoResize);

            if (ImGui.RadioButton("Third person", selectedMode == CameraDescriptor.CameraMode.BehindObject))
            {
                selectedMode = CameraDescriptor.CameraMode.BehindObject;
            }
            if (ImGui.RadioButton("First person", selectedMode == CameraDescriptor.CameraMode.FrontOfObject))
            {
                selectedMode = CameraDescriptor.CameraMode.FrontOfObject;
            }

            ImGui.End();

            if (selectedMode != camera.Mode)
            {
                camera.SetMode(selectedMode, objectPosition, objectRotation, 20f);
            }

            ImGui.SetNextWindowPos(new Vector2(10, 90), ImGuiCond.Always);
            ImGui.Begin("Player Stats", ImGuiWindowFlags.AlwaysAutoResize);
            ImGui.Text($"Health: {health}/{MaxHealth}");
            ImGui.ProgressBar((float)health / MaxHealth, new Vector2(200, 20));
            ImGui.End();

            imguiController.Render();
        }

        private static unsafe void DrawSkyBox()
        {
            var modelMatrixSkyBox = Matrix4X4.CreateScale(1000000f);
            SetModelMatrix(modelMatrixSkyBox);

            // set the texture
            int textureLocation = Gl.GetUniformLocation(program, TextureVariableName);
            if (textureLocation == -1)
            {
                throw new Exception($"{TextureVariableName} uniform not found on shader.");
            }
            // set texture 0
            Gl.Uniform1(textureLocation, 0);
            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)GLEnum.Linear);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)GLEnum.Linear);
            Gl.BindTexture(TextureTarget.Texture2D, skybox.Texture.Value);

            DrawModelObject(skybox);

            CheckError();
            Gl.BindTexture(TextureTarget.Texture2D, 0);
            CheckError();
        }

        private static unsafe void SetModelMatrix(Matrix4X4<float> modelMatrix)
        {
            SetMatrix(modelMatrix, ModelMatrixVariableName);

            // set also the normal matrix
            int location = Gl.GetUniformLocation(program, NormalMatrixVariableName);
            if (location == -1)
            {
                throw new Exception($"{NormalMatrixVariableName} uniform not found on shader.");
            }

            // G = (M^-1)^T
            var modelMatrixWithoutTranslation = new Matrix4X4<float>(modelMatrix.Row1, modelMatrix.Row2, modelMatrix.Row3, modelMatrix.Row4);
            modelMatrixWithoutTranslation.M41 = 0;
            modelMatrixWithoutTranslation.M42 = 0;
            modelMatrixWithoutTranslation.M43 = 0;
            modelMatrixWithoutTranslation.M44 = 1;

            Matrix4X4<float> modelInvers;
            Matrix4X4.Invert<float>(modelMatrixWithoutTranslation, out modelInvers);
            Matrix3X3<float> normalMatrix = new Matrix3X3<float>(Matrix4X4.Transpose(modelInvers));

            Gl.UniformMatrix3(location, 1, false, (float*)&normalMatrix);
            CheckError();
        }

        private static unsafe void SetUniform1(string uniformName, float uniformValue)
        {
            int location = Gl.GetUniformLocation(program, uniformName);
            if (location == -1)
            {
                throw new Exception($"{uniformName} uniform not found on shader.");
            }

            Gl.Uniform1(location, uniformValue);
            CheckError();
        }

        private static unsafe void SetUniform3(string uniformName, Vector3 uniformValue)
        {
            int location = Gl.GetUniformLocation(program, uniformName);
            if (location == -1)
            {
                throw new Exception($"{uniformName} uniform not found on shader.");
            }

            Gl.Uniform3(location, uniformValue);
            CheckError();
        }

        private static unsafe void DrawModelObject(ModelObjectDescriptor modelObject)
        {
            Gl.BindVertexArray(modelObject.Vao);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, modelObject.Indices);
            Gl.DrawElements(PrimitiveType.Triangles, modelObject.IndexArrayLength, DrawElementsType.UnsignedInt, null);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindVertexArray(0);
        }

        private static unsafe void SetMatrix(Matrix4X4<float> mx, string uniformName)
        {
            int location = Gl.GetUniformLocation(program, uniformName);
            if (location == -1)
            {
                throw new Exception($"{uniformName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&mx);
            CheckError();
        }

        public static void CheckError()
        {
            var error = (ErrorCode)Gl.GetError();
            if (error != ErrorCode.NoError)
                throw new Exception("GL.GetError() returned " + error.ToString());
        }

        
        private static void CheckCollisionWithTrees(double currentTime)
        {
            const float TreeCollisionRadius = 100f;
            const float PlayerCollisionRadius = 50f;

            Vector2 playerCenter = new Vector2(objectPosition.X, objectPosition.Z);     // jatekos pozicioja(csak a sikban kell)

            foreach (var tree in treePositions)     // vegigmegyek az osszes fan
            {
                Vector2 treeCenter = new Vector2(tree.X, tree.Z);       // fa pozicioja ugyanugy a sikban

                // tavolsag szamitasa
                float dx = playerCenter.X - treeCenter.X;
                float dz = playerCenter.Y - treeCenter.Y;

                float distanceSquared = dx * dx + dz * dz;
                float combinedRadius = TreeCollisionRadius + PlayerCollisionRadius;

                if (distanceSquared < combinedRadius * combinedRadius)      // utkozes eseten
                {
                    if (currentTime - lastDamageTime >= DamageCooldown)     // sebzes alkalmazasa, ha eleg ido eltelt
                    {
                        health -= 5;
                        if (health <= 0) health = 0;        // nem mehet negativba
                        lastDamageTime = currentTime;
                    }
                    break;      // egyszerre csak egy fatol tud sebzodni
                }
            }
        }
    }
}