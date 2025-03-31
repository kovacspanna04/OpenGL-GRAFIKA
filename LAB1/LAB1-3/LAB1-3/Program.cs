using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace LAB1_3
{
    internal class Program
    {
        private static IWindow graphicWindow;

        private static GL Gl;

        private static uint program;

        private static readonly string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 vPos;
		layout (location = 1) in vec4 vCol;

		out vec4 outCol;
        
        void main()
        {
			outCol = vCol;
            gl_Position = vec4(vPos.x, vPos.y, vPos.z, 1.0);
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

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "lab1-2: 2d kocka";
            windowOptions.Size = new Silk.NET.Maths.Vector2D<int>(500, 500);

            graphicWindow = Window.Create(windowOptions);

            graphicWindow.Load += GraphicWindow_Load;
            graphicWindow.Update += GraphicWindow_Update;
            graphicWindow.Render += GraphicWindow_Render;

            graphicWindow.Run();
        }

        private static void GraphicWindow_Load()
        {
            // egszeri beallitasokat
            //Console.WriteLine("Loaded");

            Gl = graphicWindow.CreateOpenGL();

            Gl.ClearColor(System.Drawing.Color.White);

            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, VertexShaderSource);
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, FragmentShaderSource);
            Gl.CompileShader(fshader);

            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);
            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);

            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }

        }

        private static void GraphicWindow_Update(double deltaTime)
        {
            // NO GL
            // make it threadsave
            //Console.WriteLine($"Update after {deltaTime} [s]");
        }

        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            //Console.WriteLine($"Render after {deltaTime} [s]");

            Gl.Clear(ClearBufferMask.ColorBufferBit);

            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            float[] vertexArray = new float[] {
                -0.6f, -0.4f, 0.0f,     // 0
                0.0f, -0.6f, 0.0f,      // 1     
                0.0f, 0.2f, 0.0f,       // 2
                -0.6f, 0.4f, 0.0f,      // 3

                0.0f, -0.6f, 0.0f,      // 4
                0.6f, -0.4f, 0.0f,      // 5
                0.6f, 0.4f, 0.0f,       // 6
                0.0f, 0.2f, 0.0f,       // 7

                0.0f, 0.2f, 0.0f,       // 8
                0.6f, 0.4f, 0.0f,       // 9
                0.0f, 0.6f, 0.0f,       // 10
                -0.6f, 0.4f, 0.0f       // 11
            };

            float[] colorArray = new float[] {
                0.5f, 0.0f, 0.5f, 1.0f,
                0.5f, 0.0f, 0.5f, 1.0f,
                0.5f, 0.0f, 0.5f, 1.0f,
                0.5f, 0.0f, 0.5f, 1.0f,

                1.0f, 0.75f, 0.8f, 1.0f,
                1.0f, 0.75f, 0.8f, 1.0f,
                1.0f, 0.75f, 0.8f, 1.0f,
                1.0f, 0.75f, 0.8f, 1.0f,

                1.0f, 0.6f, 0.6f, 1.0f,
                1.0f, 0.6f, 0.6f, 1.0f,
                1.0f, 0.6f, 0.6f, 1.0f,
                1.0f, 0.6f, 0.6f, 1.0f
            };

            uint[] indexArray = new uint[] {
                0, 1, 2,
                2, 3, 0,

                4, 5, 6,
                6, 7, 4,

                8, 9, 10,
                10, 11, 8
            };

            float[] lineVertexArray = new float[]
            {
                // vizszintes
                -0.6f, -0.133f, 0.0f,   0.0f, -0.3332f, 0.0f,   // also vonal
                -0.6f,  0.133f, 0.0f,   0.0f,  -0.0666f,   0.0f,   // felso vonal

                // fuggoleges
                -0.4f, -0.4666f, 0.0f,     -0.4f,  0.333f, 0.0f,    // bal
                -0.2f, -0.533f, 0.0f,     -0.2f,  0.266f, 0.0f,   // jobb


                0.0f, -0.3332f, 0.0f,    0.6f, -0.133f, 0.0f,
                0.0f,  -0.0666f, 0.0f,   0.6f,  0.133f, 0.0f,

                0.4f, -0.4666f, 0.0f,     0.4f,  0.333f, 0.0f,
                0.2f, -0.533f, 0.0f,      0.2f,  0.266f, 0.0f,


                -0.4f, 0.4666f, 0.0f,    0.2f,  0.266f, 0.0f,
                -0.2f, 0.53f, 0.0f,     0.4f,  0.333f, 0.0f,

                0.39f, 0.47f, 0.0f,        -0.2f,  0.266f, 0.0f,
                0.2f, 0.533f, 0.0f,     -0.4f,  0.333f, 0.0f,
            };

            uint[] lineIndexArray = new uint[]
            {
                0, 1, 2, 3,   // vizszintes vonalak
                4, 5, 6, 7,   // fuggoleges vonalak

                8, 9, 10, 11,  // vizszintes
                12, 13, 14, 15, // fuggoleges

                16, 17, 18, 19,
                20, 21, 22, 23,
            };

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(0);

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);

            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            Gl.UseProgram(program);

            Gl.DrawElements(GLEnum.Triangles, (uint)indexArray.Length, GLEnum.UnsignedInt, null); // we used element buffer
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindVertexArray(vao);


            // vonalak kirajzolasa
            //Gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Line);  // atvaltas vonalrajzolasi modba

            uint lineVao = Gl.GenVertexArray();
            Gl.BindVertexArray(lineVao);

            uint lineVertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, lineVertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)lineVertexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(0);

            uint lineIndices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, lineIndices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)lineIndexArray.AsSpan(), GLEnum.StaticDraw);

            // feher vonalak kirajzolasa
            Gl.DrawElements(GLEnum.Lines, (uint)lineIndexArray.Length, GLEnum.UnsignedInt, null);

            // visszaallitom normal rajzolasra
            //Gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Fill);

            // vonalakhoz hasznalt bufferek torlese
            Gl.DeleteBuffer(lineVertices);
            Gl.DeleteBuffer(lineIndices);
            Gl.DeleteVertexArray(lineVao);

            // always unbound the vertex buffer first, so no halfway results are displayed by accident
            Gl.DeleteBuffer(vertices);
            Gl.DeleteBuffer(colors);
            Gl.DeleteBuffer(indices);
            Gl.DeleteVertexArray(vao);
        }
    }
}
