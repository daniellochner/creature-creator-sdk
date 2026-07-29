using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace DanielLochner.CreatureCrafter.SDK
{
	public static class CustomMapSecurityValidator
	{
		static Type[] whitelistedComponents = {
			typeof(Transform),
			typeof(Collider),
			typeof(MeshFilter),
			typeof(Renderer),
			typeof(AudioSource),
			typeof(AudioReverbZone),
			typeof(AudioListener),
			typeof(ReflectionProbe),
			typeof(LightProbeGroup),
			typeof(ParticleSystem),
			typeof(ParticleSystemForceField),
			typeof(TrailRenderer),
			typeof(LineRenderer),
			typeof(Canvas),
			typeof(CanvasScaler),
			typeof(TextMeshProUGUI),
			typeof(CanvasRenderer),
			typeof(Camera),
			typeof(Light),
			typeof(MapInfo),
			typeof(Animator),
			typeof(Text),
			typeof(Rigidbody),
			typeof(Outline),
			typeof(Shadow),
			typeof(Image),
			typeof(RawImage),
			typeof(VerticalLayoutGroup),
			typeof(HorizontalLayoutGroup),
			typeof(CanvasGroup),
			typeof(TMP_Text),
			typeof(Terrain),
			typeof(TerrainCollider),
			typeof(PlatformProxy),
			typeof(SetLayer),
			typeof(SetTag),
			typeof(MinimapVisual),
			typeof(WindZone),
			typeof(WaterProxy),
			typeof(UnlockableBodyPartProxy),
			typeof(UnlockablePatternProxy),
			typeof(SpawnerProxy),
			typeof(CustomObjectProxy),
			typeof(EdibleProxy),
			typeof(HoldableProxy),
			typeof(DamageableProxy),
			typeof(ZoneProxy),
			typeof(CreatureDisplayProxy),
			typeof(KillZoneProxy),
			typeof(OutOfBoundsProxy),
			typeof(LODGroup),

#if UNITY_POST_PROCESSING_STACK_V2
			typeof(UnityEngine.Rendering.PostProcessing.PostProcessVolume),
#endif

#if UNITY_NAVIGATION
			typeof(Unity.AI.Navigation.NavMeshSurface),
			typeof(Unity.AI.Navigation.NavMeshLink),
			typeof(UnityEngine.AI.NavMeshObstacle),
#endif
		};

		static Type[] whitelistedBodyPartComponents = {
			typeof(Transform),
			typeof(Collider),
			typeof(MeshFilter),
			typeof(Renderer),
			typeof(Animator),
			typeof(Rigidbody),
			typeof(Light),
			typeof(AudioSource),
			typeof(ParticleSystem),
			typeof(ParticleSystemForceField),
			typeof(TrailRenderer),
			typeof(LineRenderer),
		};

		const BindingFlags FieldFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

		const int MaxDepth = 8;

		class Context
		{
			public Type[] Whitelist;
			public HashSet<GameObject> Visited;
			public HashSet<UnityEngine.Object> WalkedAssets = new HashSet<UnityEngine.Object>();
			public string Error = "";
		}

		public static bool IsGameObjectValid(GameObject go, out string error)
		{
			return IsGameObjectValid(go, whitelistedComponents, new HashSet<GameObject>(), out error);
		}

		public static bool IsGameObjectValid(GameObject go, HashSet<GameObject> visited, out string error)
		{
			return IsGameObjectValid(go, whitelistedComponents, visited, out error);
		}

		public static bool IsBodyPartValid(GameObject go, out string error)
		{
			return IsGameObjectValid(go, whitelistedBodyPartComponents, new HashSet<GameObject>(), out error);
		}

		public static bool IsGameObjectValid(GameObject go, Type[] whitelist, HashSet<GameObject> visited, out string error)
		{
			Context context = new Context { Whitelist = whitelist, Visited = visited };

			bool isValid = IsGameObjectValid(go, context);
			error = context.Error;
			return isValid;
		}

		public static bool IsSceneValid(Scene scene, out string error)
		{
			return IsSceneValid(scene, new HashSet<GameObject>(), out error);
		}

		public static bool IsSceneValid(Scene scene, HashSet<GameObject> visited, out string error)
		{
			Context context = new Context { Whitelist = whitelistedComponents, Visited = visited };

			foreach(var root in scene.GetRootGameObjects())
			{
				if(!IsGameObjectValid(root, context))
				{
					error = context.Error;
					return false;
				}
			}

			error = "";
			return true;
		}

		static bool IsGameObjectValid(GameObject go, Context context)
		{
			if(go == null || !context.Visited.Add(go))
			{
				return true;
			}

			foreach(var component in go.GetComponentsInChildren(typeof(Component), true))
			{
				if(component == null)
				{
					continue;
				}

				context.Visited.Add(component.gameObject);

				if(!IsWhitelisted(component, context.Whitelist))
				{
					context.Error = $"{component.gameObject} contains a non-whitelisted component ({component.GetType()})";
					return false;
				}

				if(!AreFieldsValid(component, context, 0))
				{
					return false;
				}
			}

			return true;
		}

		static bool AreFieldsValid(object owner, Context context, int depth)
		{
			if(owner == null || depth > MaxDepth)
			{
				return true;
			}

			for(Type type = owner.GetType(); type != null; type = type.BaseType)
			{
				if(type.Assembly == typeof(Component).Assembly)
				{
					continue;
				}

				foreach(var field in type.GetFields(FieldFlags))
				{
					if(!IsSerialized(field))
					{
						continue;
					}

					object value;
					try
					{
						value = field.GetValue(owner);
					}
					catch
					{
						continue;
					}

					if(!IsValueValid(value, context, depth))
					{
						return false;
					}
				}
			}

			return true;
		}

		static bool IsValueValid(object value, Context context, int depth)
		{
			if(value == null || depth > MaxDepth)
			{
				return true;
			}

			if(value is UnityEngine.Object obj)
			{
				return IsReferenceValid(obj, context, depth);
			}

			if(value is UnityEventBase unityEvent)
			{
				for(int i = 0; i < unityEvent.GetPersistentEventCount(); ++i)
				{
					if(!IsValueValid(unityEvent.GetPersistentTarget(i), context, depth + 1))
					{
						return false;
					}
				}

				return true;
			}

			if(value is IEnumerable enumerable && !(value is string))
			{
				foreach(var element in enumerable)
				{
					if(!IsValueValid(element, context, depth + 1))
					{
						return false;
					}
				}
			}

			if(!(value is Array) && IsSerializableContainer(value.GetType()))
			{
				return AreFieldsValid(value, context, depth + 1);
			}

			return true;
		}

		static bool IsReferenceValid(UnityEngine.Object obj, Context context, int depth)
		{
			if(obj == null)
			{
				return true;
			}

			GameObject go = obj as GameObject;
			if(go == null && obj is Component component)
			{
				go = component.gameObject;
			}

			if(go != null)
			{
				return IsGameObjectValid(go, context);
			}

			if(obj is ScriptableObject && context.WalkedAssets.Add(obj))
			{
				return AreFieldsValid(obj, context, depth + 1);
			}

			return true;
		}

		static bool IsWhitelisted(Component component, Type[] whitelist)
		{
			foreach(var type in whitelist)
			{
				if(type.IsInstanceOfType(component))
				{
					return true;
				}
			}

			return false;
		}

		static bool IsSerializableContainer(Type type)
		{
			if(type.IsPrimitive || type.IsEnum || type == typeof(string))
			{
				return false;
			}

			return type.IsDefined(typeof(SerializableAttribute), false) && type.Assembly != typeof(Component).Assembly;
		}

		static bool IsSerialized(FieldInfo field)
		{
			if(field.IsStatic || field.IsInitOnly)
			{
				return false;
			}

			if(field.IsPublic)
			{
				return field.GetCustomAttribute<NonSerializedAttribute>() == null;
			}

			return field.GetCustomAttribute<SerializeField>() != null
				|| field.GetCustomAttribute<SerializeReference>() != null;
		}

		public static int StripDisallowedComponents(Scene scene)
		{
			List<Component> disallowed = new List<Component>();

			foreach(var root in scene.GetRootGameObjects())
			{
				foreach(var component in root.GetComponentsInChildren(typeof(Component), true))
				{
					if(component != null && !IsWhitelisted(component, whitelistedComponents))
					{
						disallowed.Add(component);
					}
				}
			}

			int stripped = 0;

			for(int attempt = 0; attempt < 2; ++attempt)
			{
				for(int i = 0; i < disallowed.Count; ++i)
				{
					Component component = disallowed[i];
					if(component == null)
					{
						continue;
					}

					try
					{
						UnityEngine.Object.DestroyImmediate(component);
						disallowed[i] = null;
						++stripped;
					}
					catch(Exception ex)
					{
						Debug.LogWarning($"Could not strip {component.GetType()} from '{component.gameObject.name}': {ex.Message}");
					}
				}
			}

			foreach(var component in disallowed)
			{
				if(component == null)
				{
					continue;
				}

				if(component is Behaviour behaviour)
				{
					behaviour.enabled = false;
				}

				++stripped;
			}

			return stripped;
		}

		public static void SanitizeAnimators(Scene scene)
		{
			SanitizeAnimators(scene.GetRootGameObjects());
		}

		public static void SanitizeAnimators(IEnumerable<GameObject> gameObjects)
		{
			foreach(var go in gameObjects)
			{
				SanitizeAnimators(go);
			}
		}

		public static void SanitizeAnimators(GameObject go)
		{
			if(go == null)
			{
				return;
			}

			foreach(Animator animator in go.GetComponentsInChildren(typeof(Animator), true))
			{
				animator.fireEvents = false;

				if(animator.runtimeAnimatorController != null)
				{
					foreach(var clip in animator.runtimeAnimatorController.animationClips)
					{
						if(clip.events.Length > 0)
						{
							Debug.Log($"Animator on GameObject {animator.gameObject.name} had events which were removed.");
							clip.events = new AnimationEvent[0];
						}
					}
				}
			}
		}
	}
}