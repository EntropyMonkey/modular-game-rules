using UnityEngine;
using System.Collections;
using System.Xml;
using System.IO;

namespace ModularRules
{
	public class RuleParser : MonoBehaviour
	{
		public struct ActorData
		{
			public int id;
			public System.Type type;
		};

		public void Parse(RuleGenerator generator, string fileName)
		{
			TextAsset file = Resources.Load(fileName) as TextAsset;

			using (XmlReader reader = XmlReader.Create(new StringReader(file.text)))
			{
				reader.ReadToFollowing("actors");
				XmlReader actors = reader.ReadSubtree();
				ActorData current;
				while (actors.Read())
				{
					actors.ReadToFollowing("id");
					current.id = actors.ReadElementContentAsInt();

					actors.ReadToFollowing("type");
					current.type = System.Type.GetType(actors.ReadElementContentAsString());

					generator.AddActor(current);
				}
			}
		}
	}
}
