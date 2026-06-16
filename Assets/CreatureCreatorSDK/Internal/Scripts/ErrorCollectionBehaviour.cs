using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DanielLochner.CreatureCrafter.SDK
{
	public abstract class ErrorCollectionBehaviour : MonoBehaviour
	{
		public virtual void RunErrorChecks(ref List<string> errors)
		{

		}
	}
}