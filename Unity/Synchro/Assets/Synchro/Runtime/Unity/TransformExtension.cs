using System.Text;
using UnityEngine;

namespace Synchro {
  static public class TransformExtension {
    private static Recycler<StringBuilder> Recycler = new Recycler<StringBuilder>();

    static public string ToStringFullPath(this Transform transform){
      return Recycler.Once(stringBuilder => {
        stringBuilder.Clear();
        stringBuilder.Append(transform.name);
        Transform x = transform.parent;
        while (x != null){
          stringBuilder.Insert(0, "/");
          stringBuilder.Insert(0, x.name);
          x = x.parent;
        }
        return stringBuilder.ToString();
      });
    }
  }
}