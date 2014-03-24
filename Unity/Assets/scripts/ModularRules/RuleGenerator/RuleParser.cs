using UnityEngine;
using System.Collections;
using System.Xml;
using System.IO;
using System.Collections.Generic;

namespace ModularRules
{
	public class RuleParser : MonoBehaviour
	{
		public struct ActorData
		{
			public int id;
			public System.Type type;
		};

		// parameter for events or reactions
		public struct Param
		{
			public string name;
			public System.Type type;
			public object value;
		};

		public struct EventData
		{
			public int id;
			public int actorId;
			public System.Type type;

			public List<Param> parameters;
		};

		public void Parse(RuleGenerator generator, string fileName)
		{
			TextAsset file = Resources.Load(fileName) as TextAsset;
			if (file == null)
			{
				Debug.LogError("Couldn't find rule file " + fileName + " in any of the Resources folders. Aborting generation.");
				return;
			}

			using (XmlReader reader = XmlReader.Create(new StringReader(file.text)))
			{
				// read actor data
				reader.ReadToFollowing("actors");
				XmlReader actors = reader.ReadSubtree();

				ParseActors(generator, actors);

				// read event data
				reader.ReadToFollowing("events");
				XmlReader events = reader.ReadSubtree();

				ParseEvents(generator, events);

				// read reaction data
				reader.ReadToFollowing("reactions");
				XmlReader reactions = reader.ReadSubtree();

				ParseReactions(generator, reactions);
			}
		}

		private void ParseActors(RuleGenerator generator, XmlReader actors)
		{
			ActorData currentActor;
			while (actors.Read())
			{
				bool foundId = actors.ReadToFollowing("id");

				if (!foundId) break;

				currentActor.id = actors.ReadElementContentAsInt();

				actors.ReadToFollowing("type");
				currentActor.type = System.Type.GetType(actors.ReadElementContentAsString());

				generator.AddActorToScene(currentActor);
			}
		}

		private void ParseEvents(RuleGenerator generator, XmlReader events)
		{
			EventData currentEvent;
			while (events.Read())
			{
				bool foundId = events.ReadToFollowing("id");
				if (!foundId) break;

				currentEvent.id = events.ReadElementContentAsInt();

				events.ReadToFollowing("actorId");
				currentEvent.actorId = events.ReadElementContentAsInt();

				events.ReadToFollowing("type");
				currentEvent.type = System.Type.GetType(events.ReadElementContentAsString());

				currentEvent.parameters = new List<Param>();
				while (events.ReadToFollowing("param"))
				{
					Param newP = new Param { name = events.GetAttribute("name") };
					events.ReadToFollowing("type");
					string t = events.ReadElementContentAsString();
					newP.type = System.Type.GetType(t);
					//Debug.Log(newP.type);
					//Debug.Log(System.Type.GetType("UnityEngine.Camera"));

					if (newP.type == null)
					{
						Debug.LogError("Event " + currentEvent.id + ", parameter " + newP.name + ": the used type (" + t + ") doesn't exist.");
						continue;
					}

					if (newP.type.IsEnum || newP.type == typeof(Actor))
					{
						newP.value = events.ReadElementContentAsInt();
					}
					else if (newP.type == typeof(Vector3))
					{
						Vector3 v;
						string[] s = events.ReadElementContentAsString().Split(' ');
						v.x = float.Parse(s[0]);
						v.y = float.Parse(s[1]);
						v.z = float.Parse(s[2]);
					}

					currentEvent.parameters.Add(newP);
				}

				generator.AddEventToScene(currentEvent);
			}
		}

		private void ParseReactions(RuleGenerator generator, XmlReader reactions)
		{

		}
	}
}
