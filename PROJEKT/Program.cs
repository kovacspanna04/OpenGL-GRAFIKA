using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Dynamic;
using System.Numerics;
using System.Reflection;
using Szeminarium;

namespace GrafikaSzeminarium
{
    internal class Program
    {
        private static IWindow graphicWindow;

        private static GL Gl;

        private static ModelObjectDescriptor model;

        private static ModelObjectDescriptor skybox;

        private static CameraDescriptor camera = new CameraDescriptor();

        private static Vector3 modelPosition = new Vector3(0f, -20f, 0f);
        private static Vector3D<float> objectPosition = new Vector3D<float>(0, 0, 0);
        private static float objectRotation = 0f;


        private static bool isTurningLeft = false;
        private static bool isTurningRight = false;
        private static bool moveForward = false;
        private static bool moveBackward = false;
        private static bool moveLeft = false;
        private static bool moveRight = false;


        private const string ModelMatrixVariableName = "uModel";
        private const string NormalMatrixVariableName = "uNormal";
        private const string ViewMatrixVariableName = "uView";
        private const string ProjectionMatrixVariableName = "uProjection";

        private const string LightColorVariableName = "uLightColor";
        private const string LightPositionVariableName = "uLightPos";
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
            Gl.DeleteProgram(program);
        }

        private static void GraphicWindow_Load()
        {
            Gl = graphicWindow.CreateOpenGL();

            var inputContext = graphicWindow.CreateInput();
            foreach (var keyboard in inputContext.Keyboards)
            {
                keyboard.KeyDown += Keyboard_KeyDown;

                keyboard.KeyDown += (k, key, code) =>
                {
                    if (key == Key.Left)
                        isTurningLeft = true;
                    else if (key == Key.Right)
                        isTurningRight = true;
                };

                keyboard.KeyUp += (k, key, code) =>
                {
                    if (key == Key.Left)
                        isTurningLeft = false;
                    else if (key == Key.Right)
                        isTurningRight = false;
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
                    }
                };

            }

            graphicWindow.FramebufferResize += s =>
            {
                Gl.Viewport(s);
            };

            model = ModelObjectDescriptor.CreateTexturedObj(Gl);

            skybox = ModelObjectDescriptor.CreateSkyBox(Gl);

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

        private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
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
                    //camera.DecreaseZYAngle();
                    //modelRotationAngle += 0.1f; // forgás balra
                    isTurningLeft = true;
                    break;
                case Key.Right:
                    //camera.IncreaseZYAngle();
                    //modelRotationAngle -= 0.1f; // forgás jobbra
                    isTurningRight = true;
                    break;
                case Key.Down:
                    camera.IncreaseDistance();
                    break;
                case Key.Up:
                    camera.DecreaseDistance();
                    break;
                case Key.U:
                    camera.IncreaseZXAngle();
                    break;
                case Key.E:
                    camera.ToggleCameraMode();
                    break;
            }
        }

        private static void GraphicWindow_Update(double deltaTime)
        {
            // NO OpenGL
            // make it threadsafe

            float turnSpeed = 1.5f;
            //float speed = 50f * (float)deltaTime;
            float speed = 15f;

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


            if (camera.Mode == CameraDescriptor.CameraMode.BehindObject)
            {
                camera.UpdateFollowingBehind(objectPosition, objectRotation);
            }
            else
            {
                camera.UpdateCamera(objectPosition, objectRotation);
            }

            modelPosition = new Vector3(objectPosition.X, -10f, objectPosition.Z);
        }

        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit);
            Gl.Clear(ClearBufferMask.DepthBufferBit);

            Gl.UseProgram(program);

            SetUniform3(LightColorVariableName, new Vector3(1f, 1f, 1f));
            SetUniform3(LightPositionVariableName, new Vector3(7f, 7f, 7f));
            SetUniform3(ViewPositionVariableName, new Vector3(camera.Position.X, camera.Position.Y, camera.Position.Z));
            SetUniform1(ShinenessVariableName, shininess);

            var viewMatrix = Matrix4X4.CreateLookAt(camera.Position, camera.Target, camera.UpVector);
            SetMatrix(viewMatrix, ViewMatrixVariableName);

            var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>((float)(Math.PI / 2), 1024f / 768f, 0.1f, 10000000f);
            SetMatrix(projectionMatrix, ProjectionMatrixVariableName);

            DrawSkyBox();

            //var translation = Matrix4X4.CreateTranslation(0f, -20f, 0f);

            var scale = 1.7f;
            //float rotation = (float)(camera.AngleToZYPlane + Math.PI); // +180° forgatas

            var modelMatrix = Matrix4X4.CreateScale(scale) *
                  Matrix4X4.CreateRotationY(objectRotation) *
                  Matrix4X4.CreateTranslation(modelPosition.X, modelPosition.Y, modelPosition.Z);

            SetModelMatrix(modelMatrix);

            if (model.Texture.HasValue)
            {
                int textureLocation = Gl.GetUniformLocation(program, TextureVariableName);
                if (textureLocation == -1)
                    throw new Exception($"{TextureVariableName} uniform not found on shader.");

                Gl.Uniform1(textureLocation, 0); // texture unit 0
                Gl.ActiveTexture(TextureUnit.Texture0);
                Gl.BindTexture(TextureTarget.Texture2D, model.Texture.Value);
            }

            DrawModelObject(model);
        }


        private static unsafe void DrawSkyBox()
        {
            //var modelMatrixSkyBox = Matrix4X4.CreateScale(10000000f) * Matrix4X4.CreateTranslation(0f, -20f, 0f); ;
            //SetModelMatrix(modelMatrixSkyBox);

            var modelMatrixSkyBox = Matrix4X4.CreateScale(10000f);
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

        public static Vector3 ToNumerics(Vector3D<float> v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

    }
}