#define DEBUG

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

		public string[] ExtraNamespaces = { "UnityEngine", "ModularRules" };

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

				//generator.AddActorToScene(currentActor);
			}
		}

		private void ParseEvents(RuleGenerator generator, XmlReader events)
		{
			EventData currentEvent;
			while (events.ReadToFollowing("event"))
			{
				bool foundId = events.ReadToFollowing("id");
				if (!foundId) break;

				currentEvent.id = events.ReadElementContentAsInt();

				events.ReadToFollowing("label");
#if DEBUG
				Debug.Log("Processing event " + events.ReadElementContentAsString());
#endif

				events.ReadToFollowing("actorId");
				currentEvent.actorId = events.ReadElementContentAsInt();

				events.ReadToFollowing("type");
				currentEvent.type = System.Type.GetType(events.ReadElementContentAsString());

				// read until all parameters have been processed
				currentEvent.parameters = new List<Param>();
				while (events.ReadToNextSibling("param"))
				{
					Param newP = new Param { name = events.GetAttribute("name") };
					Debug.Log("reading " + newP.name);

					// get the parameter's type
					events.ReadToFollowing("type");

					string t = events.ReadElementContentAsString();

					// reflect type
					newP.type = System.Type.GetType(t);

					// try to find the type in dynamic unityengine assembly
					if (newP.type == null) 
					{
						// search all extra namespaces
						foreach (string namespc in ExtraNamespaces)
						{
							newP.type = System.Type.GetType(namespc + "." + t + "," + namespc); // for dynamic assemblies

							if (newP.type == null)
								newP.type = System.Type.GetType(namespc + "." + t); // for nondynamic assemblies
							else break;
						}

						// type couldn't be found
						if (newP.type == null)
						{
							Debug.LogError("Event " + currentEvent.id + ", parameter " + newP.name + ": the used type (" + t + ") couldn't be found.");
							events.ReadToNextSibling("param");
							continue;
						}
					}

					// get value as string
					events.ReadToFollowing("value");					
					string v = events.ReadElementContentAsString();

					// handle parameter value according to type
					if (newP.type.IsEnum || newP.type.IsSubclassOf(typeof(Actor)))
					{
						newP.value = int.Parse(v);
					}
					else if (newP.type == typeof(Vector3))
					{
						Vector3 vec;
						string[] s = events.ReadElementContentAsString().Split(' ');
						vec.x = float.Parse(s[0]);
						vec.y = float.Parse(s[1]);
						vec.z = float.Parse(s[2]);
						newP.value = vec;
					}

#if DEBUG
					Debug.Log("Adding parameter " + newP.name + ", type: " + newP.type + ", value: " + newP.value);
#endif

					currentEvent.parameters.Add(newP);
				}

				//generator.AddEventToScene(currentEvent);
			}
		}

		private void ParseReactions(RuleGenerator generator, XmlReader reactions)
		{

		}
	}
}
