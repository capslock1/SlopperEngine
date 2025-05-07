
using System.CodeDom.Compiler;
using SlopperEngine.Core;
using SlopperEngine.Core.Collections;
using SlopperEngine.Core.SceneComponents;
using SlopperEngine.Graphics.ShadingLanguage;

namespace SlopperEngine.Graphics;

/// <summary>
/// A dynamically recompiled shader, conforming to the needs of a Mesh's format and the renderer.
/// </summary>
public class SlopperShader
{
    string? _name;
    static Cache<string, SlopperShader> _shaderCache = new();
    Dictionary<(Type, MeshInfo), DrawShader> _drawCache = new(); 
    //VertexShaders do not get cached because they are unique to DrawShader's cache
    Dictionary<Type, FragmentShader> _fragmentCache = new(); 
    //can be cached and reused for unique VertexShaders
    //neither of these are using the actual Cache object, because the sloppershader instance itself should be collected

    public SyntaxTree? Scope {get; private set;}
    protected SlopperShader(SyntaxTree scope)
    {
        Scope = scope;
    }

    /// <summary>
    /// Creates a SlopperShader.
    /// </summary>
    /// <param name="filepath">The path to the .sesl file.</param>
    /// <exception cref="FileNotFoundException"></exception>
    public static SlopperShader Create(string filepath)
    {
        string path = Assets.GetPath(filepath);
        if(!File.Exists(path)) 
            throw new FileNotFoundException($"Couldnt find shader file at {filepath}");

        SlopperShader? res = _shaderCache.Get(filepath);
        if(res != null)
            return res;

        string source = File.ReadAllText(path);

        SyntaxTree scope = Transpiler.Parse(source);

        res = new(scope);
        _shaderCache.Set(filepath, res);
        res._name = filepath;
        return res;
    }

    public DrawShader GetDrawShader(MeshInfo modelFormat, RenderHandler renderer) 
    {
        if(Scope == null) 
            throw new NullReferenceException("Sloppershader had no syntaxtree associated.");

        Type rendererType = renderer.GetType();
        if(_drawCache.TryGetValue((rendererType, modelFormat), out DrawShader? result))
            return result;
        
        if(!_fragmentCache.TryGetValue(rendererType, out FragmentShader? frag))
        {
            //create frag here
            
            string fragSource;
            using(StringWriter writer = new())
            {
                using IndentedTextWriter txtWriter = new(writer);
                
                WriteFragment(txtWriter, Scope);
                
                renderer.AddFragmentMain(Scope, txtWriter);
                fragSource = writer.ToString();
            }

            //Console.WriteLine("FragSource: "+fragSource);

            frag = FragmentShader.Create(fragSource);

            _fragmentCache.Add(rendererType, frag);
        }

        //create vert here
        string vertSource;

        using(StringWriter writer = new())
        {
            using IndentedTextWriter txtWriter = new(writer);
            WriteVertex(txtWriter, Scope, modelFormat);
            renderer.AddVertexMain(Scope, txtWriter);
            vertSource = writer.ToString();
        }
        
        //Console.WriteLine("VertSource: "+vertSource);

        VertexShader vert = VertexShader.Create(vertSource);

        var res = DrawShader.Create(vert, frag);
        vert.Dispose();
        _drawCache.Add((rendererType, modelFormat), res);
        return res;
    }
    
    static void WriteInoutStructVars(List<Variable> vars, IndentedTextWriter writer)
    {
        writer.Indent++;
        foreach(Variable v in vars)
        {
            writer.Write(v.Type);
            writer.Write(' ');
            writer.Write(v.Name);
            writer.WriteLine(';');
        }
        writer.Indent--;
    }

    static void WriteVertex(IndentedTextWriter writer, SyntaxTree scope, MeshInfo info)
    {
        
        writer.Write(
@$"#version {GLInfo.Version} core
{info.GLSLGetLayoutBlock()}
void vertIn_Initialize();
{ShaderGlobals.ToGLSL()}
");

        //write vertins
        if(scope.vertIn.Count > 0)
        {
            writer.WriteLine(
@"struct SL_vertInStruct
{");
            WriteInoutStructVars(scope.vertIn, writer);
            writer.WriteLine("} vertIn;");
        }

        //write vertouts
        if(scope.vertOut.Count > 0)
        {
            writer.WriteLine(
@"struct SL_vertOutStruct
{");
            WriteInoutStructVars(scope.vertOut, writer);
            writer.WriteLine("} vertOut;");
        }

        //write verttopix
        if(scope.vertToPix.Count > 0)
        {
            writer.WriteLine(
@"out SL_vertToPix
{");
            WriteInoutStructVars(scope.vertToPix, writer);
            writer.WriteLine("} vertToPix;");
        }

        //write uniforms
        foreach(Variable j in scope.uniform)
        {
            writer.Write("uniform ");
            writer.Write(j.Type);
            writer.Write(' ');
            writer.Write(j.Name);
            writer.WriteLine(";");
        }

        info.GLSLVertexInitialize(scope, writer);

        //write functions
        foreach(Function f in scope.otherFunctions)
            f.Write(writer);
        if(scope.vertex == null) throw new NullReferenceException("yeah vertex shader is missing lol");
        scope.vertex.Write(writer);
    }

    static void WriteFragment(IndentedTextWriter writer, SyntaxTree scope)
    {
        writer.Write(
@$"#version {GLInfo.Version} core
{ShaderGlobals.ToGLSL()}
");

        //write pixout
        if(scope.pixOut.Count > 0)
        {
            writer.WriteLine(
@"struct SL_pixOut
{");
            WriteInoutStructVars(scope.pixOut, writer);
            writer.WriteLine("} pixOut;");
        }

        //write verttopix
        if(scope.vertToPix.Count > 0)
        {
            writer.WriteLine(
@"in SL_vertToPix
{");
            WriteInoutStructVars(scope.vertToPix, writer);
            writer.WriteLine("} vertToPix;");
        }

        //write uniforms
        foreach(Variable j in scope.uniform)
        {
            writer.Write("uniform ");
            writer.Write(j.Type);
            writer.Write(' ');
            writer.Write(j.Name);
            writer.WriteLine(";");
        }

        //write functions
        foreach(Function f in scope.otherFunctions)
            f.Write(writer);
        if(scope.pixel == null) throw new NullReferenceException("yeah pixel shader is missing lol");
        scope.pixel.Write(writer);
    }
}