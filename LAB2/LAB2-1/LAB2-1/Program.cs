using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using static LAB2_1.ModelObjectDescriptor;

namespace LAB2_1
{
    internal class Program
    {
        private static IWindow graphicWindow;

        private static GL Gl;

        private static ModelObjectDescriptor cube;

        private static CameraDescriptor camera = new CameraDescriptor();

        private static CubeArrangementModel cubeArrangementModel = new CubeArrangementModel();

        // egy kicsi kockat reprezental a rubik-kockabol(mindegyik egy kulon objektum)
        private class RubikCubePart
        {
            public ModelObjectDescriptor Descriptor;        // szinleiro objektum
            public Matrix4X4<float> ModelMatrix;        // a kocka vilagmatrixa(pl az eltolasa a rubik kockaban)
        }

        private static List<RubikCubePart> rubikCubes = new();      // ez fogja tartalmazni a rubik kocka osszes kis kockajat


        private const string ModelMatrixVariableName = "uModel";
        private const string ViewMatrixVariableName = "uView";
        private const string ProjectionMatrixVariableName = "uProjection";

        private static readonly string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 vPos;
		layout (location = 1) in vec4 vCol;

        uniform mat4 uModel;
        uniform mat4 uView;
        uniform mat4 uProjection;

		out vec4 outCol;
        
        void main()
        {
			outCol = vCol;
            gl_Position = uProjection*uView*uModel*vec4(vPos.x, vPos.y, vPos.z, 1.0);
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

        // letrehozza a rubik kockanak mind a 27 kicsi kockajat 
        private static void InitRubikCube()
        {
            // 27 kocka, 27 kulonbozo pozicio mindegyiknek
            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                    for (int z = 0; z < 3; z++)
                    {
                        // a kozepso kocka teljesen fekete
                        if (x == 1 && y == 1 && z == 1)
                        {
                            var descriptor = ModelObjectDescriptor.CreateCubeWithFaceColors(Gl, new Dictionary<int, (float, float, float)>());      // a dictionary ures-> minden oldala fekete
                            rubikCubes.Add(new RubikCubePart
                            {
                                Descriptor = descriptor,
                                ModelMatrix = Matrix4X4.CreateTranslation((x - 1) * 1.1f, (y - 1) * 1.1f, (z - 1) * 1.1f)
                            });
                            continue;
                        }

                        // egy random szin az egesz kockara
                        var color = RandomColor();

                        Dictionary<int, (float r, float g, float b)> faceColors = new();

                        if (y == 2) faceColors[0] = color;      // teto
                        if (z == 2) faceColors[1] = color;      // eleje
                        if (x == 0) faceColors[2] = color;      // bal
                        if (y == 0) faceColors[3] = color;      // also
                        if (z == 0) faceColors[4] = color;      // hatso
                        if (x == 2) faceColors[5] = color;      // jobb

                        var descriptorWithColor = ModelObjectDescriptor.CreateCubeWithFaceColors(Gl, faceColors);       // letrehozom a kockat a kigeneralt szinnel

                        // a kicsi kocka bekerul a rubik kocka komponenseinek listajaba
                        rubikCubes.Add(new RubikCubePart
                        {
                            Descriptor = descriptorWithColor,
                            ModelMatrix = Matrix4X4.CreateTranslation((x - 1) * 1.1f, (y - 1) * 1.1f, (z - 1) * 1.1f)       // bekerul a helyere, es az 1.1f el oldom meg, hogy legyen kozottuk egy kis hely
                        });
                    }
        }

        private static (float r, float g, float b) RandomColor()
        {
            var rand = new Random();
            return (rand.NextSingle(), rand.NextSingle(), rand.NextSingle());       // r, g, b random mindegyik
        }

        private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            switch (key)
            {
                case Key.Left:
                    camera.DecreaseZYAngle();
                    break;
                case Key.Right:
                    camera.IncreaseZYAngle();
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
                case Key.D:
                    camera.DecreaseZXAngle();
                    break;
                case Key.Space:
                    cubeArrangementModel.AnimationEnabled = !cubeArrangementModel.AnimationEnabled;
                    break;
            }
        }

        private static void GraphicWindow_Update(double deltaTime)
        {
            // NO OpenGL
            // make it threadsafe
            cubeArrangementModel.AdvanceTime(deltaTime);
        }

        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit);
            Gl.Clear(ClearBufferMask.DepthBufferBit);

            Gl.UseProgram(program);

            var viewMatrix = Matrix4X4.CreateLookAt(camera.Position, camera.Target, camera.UpVector);
            SetMatrix(viewMatrix, ViewMatrixVariableName);

            var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>((float)(Math.PI / 2), 1024f / 768f, 0.1f, 100f);
            SetMatrix(projectionMatrix, ProjectionMatrixVariableName);


            var modelMatrixCenterCube = Matrix4X4.CreateScale((float)cubeArrangementModel.CenterCubeScale);
            SetMatrix(modelMatrixCenterCube, ModelMatrixVariableName);

            // minden kis reszkockahoz beallitom a modellmatrixot es kirajzolom
            foreach (var part in rubikCubes)
            {
                SetMatrix(part.ModelMatrix, ModelMatrixVariableName);
                DrawModelObject(part.Descriptor);
            }


            Matrix4X4<float> diamondScale = Matrix4X4.CreateScale(0.25f);
            Matrix4X4<float> rotx = Matrix4X4.CreateRotationX((float)Math.PI / 4f);
            Matrix4X4<float> rotz = Matrix4X4.CreateRotationZ((float)Math.PI / 4f);
            Matrix4X4<float> roty = Matrix4X4.CreateRotationY((float)cubeArrangementModel.DiamondCubeLocalAngle);
            Matrix4X4<float> trans = Matrix4X4.CreateTranslation(1f, 1f, 0f);
            Matrix4X4<float> rotGlobalY = Matrix4X4.CreateRotationY((float)cubeArrangementModel.DiamondCubeGlobalYAngle);
            Matrix4X4<float> dimondCubeModelMatrix = diamondScale * rotx * rotz * roty * trans * rotGlobalY;
            SetMatrix(dimondCubeModelMatrix, ModelMatrixVariableName);
            
            // itt ugyanaz van
            foreach (var part in rubikCubes)
            {
                SetMatrix(part.ModelMatrix, ModelMatrixVariableName);
                DrawModelObject(part.Descriptor);
            }


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
                throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
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
    }
}
