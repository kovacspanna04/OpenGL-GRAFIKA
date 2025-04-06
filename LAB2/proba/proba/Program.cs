using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace proba
{
    internal class Program
    {
        private static IWindow graphicWindow;

        private static GL Gl;

        private static ModelObjectDescriptor cube;

        private static CameraDescriptor camera = new CameraDescriptor();

        private static CubeArrangementModel cubeArrangementModel = new CubeArrangementModel();

        private static float currentRotation = 0f;
        private static float targetRotation = 0f;
        private static readonly Vector3D<float> LayerCenter = new(0, -1.1f, 0); // also reteg kozeppontja


        // egy kicsi kockat reprezental a rubik-kockabol(mindegyik egy kulon objektum)
        private class RubikCubePart
        {
            public ModelObjectDescriptor Descriptor;        // szinleiro objektum
            public Matrix4X4<float> ModelMatrix;        // a kocka vilagmatrixa(pl az eltolasa a rubik kockaban)
            public (int x, int y, int z) LogicalPosition;   // ez segit a szelet kivalasztasanal
        }

        private static List<RubikCubePart> rubikCubes = new();      // ez fogja tartalmazni a rubik kocka osszes kis kockajat


        private const string ModelMatrixVariableName = "uModel";
        private const string ViewMatrixVariableName = "uView";
        private const string ProjectionMatrixVariableName = "uProjection";

        private static readonly string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 vPos;
		layout (location = 1) in vec4 vCol;

        uniform mat4 uLayerRotation;
        uniform mat4 uModel;
        uniform mat4 uView;
        uniform mat4 uProjection;

		out vec4 outCol;
        
        void main()
        {
			outCol = vCol;
            gl_Position = uProjection * uView * uLayerRotation * uModel * vec4(vPos, 1.0);
        }
        ";


        private static readonly string FragmentShaderSource = @"
        #version 330 core
        out vec4 FragColor;
		
		in vec4 outCol;

        void main()
        {
            FragColor = outCol;
        }
        ";

        private static uint program;

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "Grafika LAB2-1";
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
            cube.Dispose();
            Gl.DeleteProgram(program);
        }

        private static void GraphicWindow_Load()
        {
            Gl = graphicWindow.CreateOpenGL();

            var inputContext = graphicWindow.CreateInput();
            foreach (var keyboard in inputContext.Keyboards)
            {
                keyboard.KeyDown += Keyboard_KeyDown;
            }

            cube = ModelObjectDescriptor.CreateCube(Gl);

            Gl.ClearColor(System.Drawing.Color.White);

            Gl.Enable(EnableCap.CullFace);
            Gl.CullFace(TriangleFace.Back);

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);


            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, VertexShaderSource);
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, FragmentShaderSource);
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

            InitRubikCube();            // itt meghivtam az inicializalast
        }

        private static void InitRubikCube()
        {
            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                    for (int z = 0; z < 3; z++)
                    {
                        Dictionary<int, (float r, float g, float b)> faceColors = new();

                        if (y == 2) faceColors[0] = (1f, 1f, 1f);   // teto - feher
                        if (y == 0) faceColors[3] = (1f, 1f, 0f);   // alja - sarga
                        if (z == 2) faceColors[1] = (0f, 0f, 1f);   // eleje - kek
                        if (z == 0) faceColors[4] = (0f, 1f, 0f);   // hata - zold
                        if (x == 0) faceColors[2] = (1f, 0f, 0f);   // bal - piros
                        if (x == 2) faceColors[5] = (1f, 0.5f, 0f); // jobb - narancssarga

                        var descriptor = ModelObjectDescriptor.CreateCubeWithFaceColors(Gl, faceColors);    // ha egyik sem teljesul a fentiekbol, akkor a dictionary alapbol ures lesz, es akkor magatol fekete szint ad neki

                        rubikCubes.Add(new RubikCubePart
                        {
                            Descriptor = descriptor,
                            ModelMatrix = Matrix4X4.CreateTranslation((x - 1) * 1.1f, (y - 1) * 1.1f, (z - 1) * 1.1f),
                            LogicalPosition = (x, y, z)
                        });
                    }
        }

        private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            switch (key)
            {
                case Key.W: camera.MoveForward(); break;
                case Key.S: camera.MoveBackward(); break;
                case Key.A: camera.MoveLeft(); break;
                case Key.D: camera.MoveRight(); break;
                case Key.Q: camera.MoveUp(); break;
                case Key.E: camera.MoveDown(); break;
                case Key.Left: camera.DecreaseZYAngle(); break;
                case Key.Right: camera.IncreaseZYAngle(); break;
                case Key.Up: camera.DecreaseZXAngle(); break;
                case Key.Down: camera.IncreaseZXAngle(); break;
                case Key.Space: targetRotation += 90f; break;
                case Key.Backspace: targetRotation -= 90f; break;
            }

        }

        private static void GraphicWindow_Update(double deltaTime)
        {
            // NO OpenGL
            // make it threadsafe
            cubeArrangementModel.AdvanceTime(deltaTime);

            if (Math.Abs(currentRotation - targetRotation) > 0.01f)
            {
                float step = (float)(deltaTime * 90f);      // 90 fok / masodperc
                if (currentRotation < targetRotation)
                    currentRotation = Math.Min(currentRotation + step, targetRotation);
                else
                    currentRotation = Math.Max(currentRotation - step, targetRotation);
            }
        }

        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit);
            Gl.Clear(ClearBufferMask.DepthBufferBit);
            Gl.UseProgram(program);

            var viewMatrix = Matrix4X4.CreateLookAt(camera.Position, camera.Target, camera.UpVector);
            SetMatrix(viewMatrix, ViewMatrixVariableName);

            SetMatrix(viewMatrix, ViewMatrixVariableName);

            var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>((float)(Math.PI / 2), 1024f / 768f, 0.1f, 100f);
            SetMatrix(projectionMatrix, ProjectionMatrixVariableName);


            var modelMatrixCenterCube = Matrix4X4.CreateScale((float)cubeArrangementModel.CenterCubeScale);
            SetMatrix(modelMatrixCenterCube, ModelMatrixVariableName);

            Matrix4X4<float> diamondScale = Matrix4X4.CreateScale(0.25f);
            Matrix4X4<float> rotx = Matrix4X4.CreateRotationX((float)Math.PI / 4f);
            Matrix4X4<float> rotz = Matrix4X4.CreateRotationZ((float)Math.PI / 4f);
            Matrix4X4<float> roty = Matrix4X4.CreateRotationY((float)cubeArrangementModel.DiamondCubeLocalAngle);
            Matrix4X4<float> trans = Matrix4X4.CreateTranslation(1f, 1f, 0f);
            Matrix4X4<float> rotGlobalY = Matrix4X4.CreateRotationY((float)cubeArrangementModel.DiamondCubeGlobalYAngle);
            Matrix4X4<float> dimondCubeModelMatrix = diamondScale * rotx * rotz * roty * trans * rotGlobalY;
            SetMatrix(dimondCubeModelMatrix, ModelMatrixVariableName);

            var layerRotationMatrix = GetLayerRotationMatrix();

            foreach (var part in rubikCubes)
            {
                SetMatrix(part.ModelMatrix, ModelMatrixVariableName);

                if (part.LogicalPosition.y == 0)
                {
                    SetMatrix(layerRotationMatrix, "uLayerRotation");
                }
                else
                {
                    SetMatrix(Matrix4X4<float>.Identity, "uLayerRotation");
                }
                DrawModelObject(part.Descriptor);
            }
        }

        private static unsafe void SetMatrix(Matrix4X4<float> mx, string uniformName)
        {
            int location = Gl.GetUniformLocation(program, uniformName);
            if (location == -1)
            {
                throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&mx);
            CheckError();
        }


        private static Matrix4X4<float> GetLayerRotationMatrix()
        {
            float radians = (float)(currentRotation * Math.PI / 180);
            var T1 = Matrix4X4.CreateTranslation(-LayerCenter);
            var R = Matrix4X4.CreateRotationY(radians);         // y==0 reteg eseten
            var T2 = Matrix4X4.CreateTranslation(LayerCenter);

            return T1 * R * T2;
        }

        private static unsafe void DrawModelObject(ModelObjectDescriptor modelObject)
        {
            Gl.BindVertexArray(modelObject.Vao);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, modelObject.Indices);
            Gl.DrawElements(PrimitiveType.Triangles, modelObject.IndexArrayLength, DrawElementsType.UnsignedInt, null);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindVertexArray(0);
        }

        public static void CheckError()
        {
            var error = (ErrorCode)Gl.GetError();
            if (error != ErrorCode.NoError)
                throw new Exception("GL.GetError() returned " + error.ToString());
        }
    }
}
