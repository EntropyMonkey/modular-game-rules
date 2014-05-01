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
		#region XML Parsing
		[HideInInspector]
		public string[] ExtraNamespaces = { "System", "UnityEngine", "ModularRules" };

		public void Parse(RuleGenerator generator, string filename)
		{

			string filepath = Directory.GetCurrentDirectory() + @"/Assets/Resources/" + filename + ".xml";
			XDocument xmlDoc = XDocument.Load(filepath);

			if (xmlDoc == null)
			{
				Debug.LogError("Couldn't find rule file " + filename + " in the Resources folders. Aborting generation.");
				return;
			}

			// parse actors
			ParseActors(generator, xmlDoc.Element("rules").Element("actors"));

			// parse events
			ParseEvents(generator, xmlDoc.Element("rules").Element("events"));

			// parse reactions
			ParseReactions(generator, xmlDoc.Element("rules").Element("reactions"));
		}

		private void ParseActors(RuleGenerator generator, XElement xActors)
		{
			BaseRuleElement.ActorData currentActor = new BaseRuleElement.ActorData();
			foreach (XElement xActor in xActors.Elements("actor"))
			{
				currentActor.id = int.Parse(xActor.Element("id").Value);

				currentActor.type = ReflectOverSeveralNamespaces(xActor.Element("type").Value, ExtraNamespaces);

				currentActor.parameters = ParseParameters(xActor);

				// parse component parameters
				currentActor.components = new List<BaseRuleElement.ComponentData>();
				foreach (XElement xComponent in xActor.Elements("component"))
				{
					BaseRuleElement.ComponentData currentComponent = new BaseRuleElement.ComponentData()
					{
						type = ReflectOverSeveralNamespaces(xComponent.Element("type").Value, ExtraNamespaces)
					};

					currentComponent.parameters = ParseParameters(xComponent);
					currentActor.components.Add(currentComponent);
				}

				generator.AddActorToScene(currentActor);
			}
		}

		private void ParseEvents(RuleGenerator generator, XElement xEvents)
		{
			BaseRuleElement.EventData currentEvent = new BaseRuleElement.EventData();
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
			BaseRuleElement.ReactionData currentReaction = new BaseRuleElement.ReactionData();
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
		private List<BaseRuleElement.Param> ParseParameters(XElement xElement)
		{
			List<BaseRuleElement.Param> parameters = new List<BaseRuleElement.Param>();

			foreach (XElement xParam in xElement.Elements("param"))
			{
				BaseRuleElement.Param newP = new BaseRuleElement.Param { name = xParam.Attribute("name").Value };

				// get the parameter's type
				string t = xParam.Element("type").Value;

				newP.type = ReflectOverSeveralNamespaces(t, ExtraNamespaces);

				// get value as string
				string v = xParam.Element("value").Value;

				float tryparsef; int tryparsei; bool tryparseb;

				// handle parameter value according to type
				if (newP.type.IsSubclassOf(typeof(Actor)) || newP.type.IsAssignableFrom(typeof(Actor)))
				{
					newP.value = int.Parse(v);
				}
				else if (newP.type.IsEnum)
				{
					newP.value = Enum.Parse(newP.type, v);
				}
				else if (float.TryParse(v, out tryparsef))
				{
					newP.value = float.Parse(v);
				}
				else if (int.TryParse(v, out tryparsei))
				{
					newP.value = int.Parse(v);
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
				else if (bool.TryParse(v, out tryparseb))
				{
					newP.value = bool.Parse(v);
				}
				else if (newP.type == typeof(string))
				{
					newP.value = v;
				}
				else if (newP.type == typeof(List<string>))
				{
					List<string> strings = new List<string>();
					strings.AddRange(v.Split(' '));
					newP.value = strings;
				}
				else
				{
					Debug.LogError("Element " + xElement.Element("id").Value + ", parameter " + newP.name + ": the used type (" + t + ") couldn't be interpreted.");
				}

#if DEBUG
				Debug.Log("Adding parameter " + newP.name + ", type: " + newP.type + ", value: " + newP.value);
#endif

				parameters.Add(newP);
			}

			return parameters;
		}
		#endregion

		#region Saving Rules
		public void SaveRules(List<BaseRuleElement.RuleData> data, string filename)
		{
			string filepath = Directory.GetCurrentDirectory() + @"/Assets/Resources/" + filename + ".xml";

			XDocument xmlDoc = new XDocument();
			if (File.Exists(filepath))
			{
				string newName = filename.Split('_')[0] + "_" + (int.Parse(filename.Split('_')[1]) + 1);
				Debug.Log("File exists. Trying again: " + newName);

				SaveRules(data, newName);
				return;
			}
			else
			{
				Debug.Log("Couldn't find rule file " + filename + " in the Resources folder. Creating..");
			}

			Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"/Assets/Resources");

			XDeclaration declaration = new XDeclaration("1.0", null, null);
			xmlDoc.Declaration = declaration;
			xmlDoc.Add(new XElement("rules"));
			xmlDoc.Save(filepath);

			xmlDoc.Element("rules").Add(new XElement("actors"));
			XElement xActors = xmlDoc.Element("rules").Element("actors");

			xmlDoc.Element("rules").Add(new XElement("events"));
			XElement xEvents = xmlDoc.Element("rules").Element("events");

			xmlDoc.Element("rules").Add(new XElement("reactions"));
			XElement xReactions = xmlDoc.Element("rules").Element("reactions");

			foreach (BaseRuleElement.RuleData ruleData in data)
			{
				if (ruleData != null)
				{
					if (ruleData.GetType() == typeof(BaseRuleElement.ActorData))
					{
						BaseRuleElement.ActorData actorData = ruleData as BaseRuleElement.ActorData;
						XElement xActor = new XElement("actor");
						xActor.Add(new XElement("id") { Value = "" + ruleData.id });
						xActor.Add(new XElement("type") { Value = GetTypeStringWithoutNamespaces(actorData.type) });
						xActor.Add(new XElement("label") { Value = actorData.label });

						AddParameters(xActor, actorData.parameters);

						AddComponents(xActor, actorData.components);

						xActors.Add(xActor);
					}
					else if (ruleData.GetType() == typeof(BaseRuleElement.EventData))
					{
						BaseRuleElement.EventData eventData = ruleData as BaseRuleElement.EventData;
						XElement xEvent = new XElement("event");
						xEvent.Add(new XElement("id") { Value = "" + ruleData.id });

						xEvent.Add(new XElement("type") { Value = GetTypeStringWithoutNamespaces(eventData.type) });
						xEvent.Add(new XElement("label") { Value = eventData.label });

						xEvent.Add(new XElement("actorId") { Value = "" + eventData.actorId });

						AddParameters(xEvent, eventData.parameters);

						xEvents.Add(xEvent);
					}
					else if (ruleData.GetType() == typeof(BaseRuleElement.ReactionData))
					{
						XElement xReaction = new XElement("reaction");
						xReaction.Add(new XElement("id") { Value = "" + ruleData.id });

						BaseRuleElement.ReactionData reactionData = ruleData as BaseRuleElement.ReactionData;

						xReaction.Add(new XElement("type") { Value = GetTypeStringWithoutNamespaces(reactionData.type) });
						xReaction.Add(new XElement("label") { Value = reactionData.label });

						xReaction.Add(new XElement("actorId") { Value = "" + reactionData.actorId });
						xReaction.Add(new XElement("listenedEventId") { Value = "" + reactionData.eventId });

						AddParameters(xReaction, reactionData.parameters);

						xReactions.Add(xReaction);
					}

				}
			}

			xmlDoc.Save(filepath);
		}

		string GetTypeStringWithoutNamespaces(Type type)
		{
			string result = "" + type;

			if (result.Contains("."))
			{
				int index = result.LastIndexOf('.');

				result = result.Substring(result.LastIndexOf('.') + 1, result.Length - (index + 1));

			}

			Debug.Log(result);

			return result;
		}

		void AddComponents(XElement element, List<BaseRuleElement.ComponentData> components)
		{
			if (components == null) return;

			foreach (BaseRuleElement.ComponentData component in components)
			{
				XElement xComp = new XElement("component");
				xComp.Add(new XElement("type") { Value = GetTypeStringWithoutNamespaces(component.type) });

				AddParameters(xComp, component.parameters);

				element.Add(xComp);
			}
		}

		void AddParameters(XElement element, List<BaseRuleElement.Param> parameters)
		{
			if (parameters == null) return;

			foreach (BaseRuleElement.Param param in parameters)
			{
				XElement xParam = new XElement("param");
				xParam.Add(new XAttribute("name", param.name));

				xParam.Add(new XElement("type") { Value = GetTypeStringWithoutNamespaces(param.type) });
				if (param.type.IsEnum)
					xParam.Add(new XElement("value") { Value = "" + (int)param.value });
				else
					xParam.Add(new XElement("value") { Value = "" + param.value });

				element.Add(xParam);
			}
		}
		#endregion

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
					{
						type = System.Type.GetType(namespc + "." + typeName); // for nondynamic assemblies
						
						if (type != null) break;
					}
					else break;
				}

				// type couldn't be found
				if (type == null)
				{
					Debug.LogError("The used type (" + typeName + ") couldn't be found via reflection.");
				}
			}

			return type;
		}
		#endregion
	}
};
