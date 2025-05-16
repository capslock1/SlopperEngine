using System.CodeDom.Compiler;

namespace SlopperEngine.SceneObjects;

partial class SceneObject
{
    public static class DebugFunctions
    {
        /// <summary>
        /// Writes out all children of an object, public or private.
        /// </summary>
        public static void WriteOutTree(SceneObject obj)
        {
            TextWriter w = new StringWriter();
            IndentedTextWriter wr = new(w);
            RecursiveWriteTree(obj);
            System.Console.WriteLine(w);

            void RecursiveWriteTree(SceneObject obj)
            {
                wr.WriteLine(obj.GetType().Name);
                wr.Write("registry complete: ");
                wr.WriteLine(obj._registryComplete);
                if (obj._childLists == null) return;
                wr.Indent++;
                foreach (var l in obj._childLists)
                {
                    wr.Write(l.GetType().Name);
                    wr.WriteLine(':');
                    wr.Indent++;
                    for (int i = 0; i < l.Count; i++)
                        RecursiveWriteTree(l.Get(i));
                    wr.Indent--;
                }
                wr.Indent--;
            }
        }

        /// <summary>
        /// Gets the amount of public and private children a SceneObject has.
        /// </summary>
        public static int RecursiveChildCount(SceneObject obj)
        {
            int res = 0;
            if (obj._childLists == null) return 0;
            foreach (var l in obj._childLists)
            {
                res += l.Count;
                for (int i = 0; i < l.Count; i++)
                    res += RecursiveChildCount(l.Get(i));
            }
            return res;
        }
    }
}