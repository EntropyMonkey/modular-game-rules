using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;
using System;

namespace ModularRules
{
	public class RuleParserLinq : MonoBehaviour
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
			public string label;

			public List<Param> parameters;
		};

		public struct ReactionData
		{
			public int id;
			public int eventId;
			public int actorId;
			public System.Type type;
			public string label;

			public List<Param> parameters;
		};

		[HideInInspector]
		public string[] ExtraNamespaces = { "UnityEngine", "ModularRules" };

		public void Parse(RuleGenerator generator, string fileName)
		{

			TextAsset file = Resources.Load(fileName) as TextAsset;
			if (file == null)
			{
				Debug.LogError("Couldn't find rule file " + fileName + " in any of the Resources folders. Aborting generation.");
				return;
			}

			XDocument xmlDoc = XDocument.Parse(file.text);

			// parse actors
			ParseActors(generator, xmlDoc.Element("rules").Element("actors"));

			// parse events
			ParseEvents(generator, xmlDoc.Element("rules").Element("events"));

			// parse reactions
			ParseReactions(generator, xmlDoc.Element("rules").Element("reactions"));
		}

		private void ParseActors(RuleGenerator generator, XElement xActors)
		{
			ActorData currentActor;
			foreach (XElement xActor in xActors.Elements("actor"))
			{
				currentActor.id = int.Parse(xActor.Element("id").Value);

				currentActor.type = ReflectOverSeveralNamespaces(xActor.Element("type").Value, ExtraNamespaces);

				generator.AddActorToScene(currentActor);
			}
		}

		private void ParseEvents(RuleGenerator generator, XElement xEvents)
		{
			EventData currentEvent;
			foreach(XElement xEvent in xEvents.Elements("event"))
			{
				currentEvent.id = int.Parse(xEvent.Element("id").Value);
				currentEvent.label = xEvent.Element("label").Value;

#if DEBUG
				Debug.Log("Processing event " + currentEvent.label + ".");
#endif

				currentEvent.actorId = int.Parse(xEvent.Element("actorId").Value);

				currentEvent.type = ReflectOverSeveralNamespaces(xEvent.Element("type").Value, ExtraNamespaces);

				// get parameters with right types
				currentEvent.parameters = ParseParameters(xEvent);
				
				// add event to scene
				generator.AddEventToScene(currentEvent);
			}
		}

		private void ParseReactions(RuleGenerator generator, XElement xReactions)
		{
			ReactionData currentReaction;
			foreach (XElement xReaction in xReactions.Elements("reaction"))
			{
				currentReaction.id = int.Parse(xReaction.Element("id").Value);
				currentReaction.label = xReaction.Element("label").Value;
				
#if DEBUG
				Debug.Log("Processing reaction " + currentReaction.label + ".");
#endif

				currentReaction.eventId = int.Parse(xReaction.Element("listenedEventId").Value);
				currentReaction.actorId = int.Parse(xReaction.Element("actorId").Value);

				currentReaction.type = ReflectOverSeveralNamespaces(xReaction.Element("type").Value, ExtraNamespaces);

				currentReaction.parameters = ParseParameters(xReaction);

				generator.AddReactionToScene(currentReaction);
			}
		}

		// parses all parameters of this element and sets up their values with the right types
		private List<Param> ParseParameters(XElement xElement)
		{
			List<Param> parameters = new List<Param>();

			foreach (XElement xParam in xElement.Elements("param"))
			{
				Param newP = new Param { name = xParam.Attribute("name").Value };

				// get the parameter's type
				string t = xParam.Element("type").Value;

				newP.type = ReflectOverSeveralNamespaces(t, ExtraNamespaces);

				// get value as string
				string v = xParam.Element("value").Value;

				// handle parameter value according to type
				if (newP.type.IsSubclassOf(typeof(Actor)))
				{
					newP.value = int.Parse(v);
				}
				else if (newP.type.IsEnum)
				{
					newP.value = Enum.Parse(newP.type, v);
				}
				else if (newP.type == typeof(Vector3))
				{
					Vector3 vec;
					string[] s = v.Split(' ');
					vec.x = float.Parse(s[0]);
					vec.y = float.Parse(s[1]);
					vec.z = float.Parse(s[2]);
					newP.value = vec;
				}
				else
				{
					Debug.LogError("Element " + xElement.Element("id").Value + ", parameter " + newP.name + ": the used type (" + t + ") couldn't be reflected.");
				}

#if DEBUG
				Debug.Log("Adding parameter " + newP.name + ", type: " + newP.type + ", value: " + newP.value);
#endif

				parameters.Add(newP);
			}

			return parameters;
		}

		#region Reflection Helpers
		private System.Type ReflectOverSeveralNamespaces(string typeName, string[] namespaces)
		{
			// reflect type
			System.Type type = System.Type.GetType(typeName);

			// try to find the type in dynamic unityengine assembly
			if (type == null)
			{
				// search all extra namespaces
				foreach (string namespc in namespaces)
				{
					type = System.Type.GetType(namespc + "." + typeName + "," + namespc); // for dynamic assemblies

					if (type == null)
						type = System.Type.GetType(namespc + "." + typeName); // for nondynamic assemblies
					else break;
				}

				// type couldn't be found
				if (type == null)
				{
					Debug.LogError("The used type (" + typeName + ") couldn't be found.");
				}
			}

			return type;
		}
		#endregion
	}
};
