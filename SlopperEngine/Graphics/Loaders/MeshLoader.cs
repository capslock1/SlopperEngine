using OpenTK.Mathematics;
using SlopperEngine.Core;
using SlopperEngine.Core.Collections;
using SlopperEngine.Graphics.GPUResources;
using SlopperEngine.Graphics.GPUResources.Meshes;

namespace SlopperEngine.Graphics.Loaders;

/// <summary>
/// Loads meshes into VRAM from the disk.
/// </summary>
public static class MeshLoader
{
    static Cache<string, Mesh> _loadedMeshes = new();
    /// <summary>
    /// Doesn't work.
    /// </summary>
    /// <param name="filepath">Don't even worry about it.</param>
    /// <returns>Literally nothing.</returns>
    public static Mesh? GetMesh(string filepath)
    {
        Mesh? result = _loadedMeshes.Get(filepath);
        if(result != null)
            return result;
        
        Assimp.Scene scene;
        try
        {
            Assimp.AssimpContext context = new Assimp.AssimpContext();
            scene = context.ImportFile(filepath);
        }
        catch(FileNotFoundException)
        {
            Console.WriteLine("Meshloader: Could not find file at "+filepath);
            return null;
        }
        if(scene.MeshCount <= 0) 
        {
            Console.WriteLine("Meshloader: could not find any meshes in "+filepath);
            return null;
        }

        var res = scene.Meshes[0];
        //if(res.has)
        //uint[] indeces = res.GetUnsignedIndices();
        //res.Bones[3].VertexWeights[3].

        return result;
    }

    /// <summary>
    /// Loads a wavefront OBJ mesh from a filepath.
    /// </summary>
    /// <param name="filename">The path of the file. Is relative to the full game path.</param>
    /// <returns>A new Mesh instance, or an instance from the cache.</returns>
    /// <exception cref="Exception"></exception>
    public static Mesh SimpleFromWavefrontOBJ(string filepath)
    {
        Mesh? result = _loadedMeshes.Get(filepath);
        if(result != null)
            return result;
        
        List<Vector3> posData = new List<Vector3>();
        List<Vector2> uvData = new List<Vector2>();
        List<Vector3> normData = new List<Vector3>();
        List<Vector3i> indices = new List<Vector3i>();
        
        string filename = Assets.GetPath(filepath);
        if (!File.Exists(filename))
            throw new Exception("Model could not be found at filepath " + filename);
        if (filename.Substring(filename.Length - 4) != ".obj")
            throw new Exception("Specified model is not in Wavefront (.obj) format. Model in question: " + filename);

        StreamReader reader = new StreamReader(filename);
        string? line = reader.ReadLine();
        while (line != null)
        {
            string[] words;
            if (line.Length < 2)
            {
                line = reader.ReadLine();
                continue;
            }
            switch (line.Substring(0, 2))
            {
                default:
                    break;
                case "v ":
                    //v is for position
                    words = line.Split(' ');
                    if (words.Length != 4) break;

                    posData.Add(new Vector3(
                        float.Parse(words[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture), 
                        float.Parse(words[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture), 
                        float.Parse(words[3], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture)));
                    break;

                case "vn":
                    //vn is for normal
                    words = line.Split(' ');
                    if (words.Length != 4) break;

                    normData.Add(new Vector3(
                        float.Parse(words[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture), 
                        float.Parse(words[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture), 
                        float.Parse(words[3], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture)));
                    break;
                case "vt":
                    //t is for... uvs? what
                    words = line.Split(' ');
                    if (words.Length != 3) break;

                    uvData.Add(new Vector2(
                        float.Parse(words[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture),
                        float.Parse(words[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture)));
                    break;
                case "f ":

                    //data is vert, UV and normal indeces, assuming 3 or 4 sets of 3 ints
                    words = line.Split(' ');
                    if(words.Length == 4)
                    {
                        //this is ONE triangle
                        for(int i = 1; i < 4; i++)
                        {
                            string[] ints = words[i].Split('/');
                            indices.Add(new Vector3i(
                                int.Parse(ints[0])-1, 
                                int.Parse(ints[1])-1, 
                                int.Parse(ints[2])-1));
                        }
                    }
                    if(words.Length == 5)
                    {
                        string[] ints;
                        //this is TWO triangle
                        for(int i = 1; i < 4; i++)
                        {
                            ints = words[i].Split('/');
                            indices.Add(new Vector3i(
                                int.Parse(ints[0])-1, 
                                int.Parse(ints[1])-1, 
                                int.Parse(ints[2])-1));
                        }
                        ints = words[1].Split('/');
                        indices.Add(new Vector3i(
                            int.Parse(ints[0])-1, 
                            int.Parse(ints[1])-1, 
                            int.Parse(ints[2])-1));
                        for(int i = 3; i < 5; i++)
                        {
                            ints = words[i].Split('/');
                            indices.Add(new Vector3i(
                                int.Parse(ints[0])-1, 
                                int.Parse(ints[1])-1, 
                                int.Parse(ints[2])-1));
                        }
                    }
                    break;
            }
            line = reader.ReadLine();
        }
        reader.Close();
        
        Dictionary<Vector3i, uint> vertIndices = new Dictionary<Vector3i, uint>();
        List<uint> finalIndices = new List<uint>();
        List<float> finalVertices = new List<float>();
        uint fIndex = 0;
        foreach(var vertIndex in indices)
        {
            if(!vertIndices.TryGetValue(vertIndex, out uint index))
            {
                //this unique pairing of pos index, uv index and normal index has not been indexed yet.
                //add it to the dictionary and the final indices
                vertIndices.Add(vertIndex, fIndex);
                finalIndices.Add(fIndex);

                //position
                finalVertices.Add(posData[vertIndex.X].X);
                finalVertices.Add(posData[vertIndex.X].Y);
                finalVertices.Add(posData[vertIndex.X].Z);

                //UVs
                finalVertices.Add(uvData[vertIndex.Y].X);
                finalVertices.Add(uvData[vertIndex.Y].Y);

                //normals
                finalVertices.Add(normData[vertIndex.Z].X);
                finalVertices.Add(normData[vertIndex.Z].Y);
                finalVertices.Add(normData[vertIndex.Z].Z);

                fIndex++;
            }
            else finalIndices.Add(index);
        }
        
        result = new OBJMesh(finalVertices.ToArray(), finalIndices.ToArray());
        _loadedMeshes.Set(filepath, result);
        result.OverrideOrigin = new OBJMeshOrigin(filepath);
        return result;
    }

    class OBJMeshOrigin(string filepath) : IGPUResourceOrigin
    {
        string filepath = filepath;
        public GPUResource CreateResource() => SimpleFromWavefrontOBJ(filepath);
    }
}