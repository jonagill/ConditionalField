using System;
using UnityEngine;

namespace ConditionalField 
{
	/// <summary>
	/// Helper class for configuring ConditionalAttributes (with a conveniently shorter name)
	/// </summary>
	public static class Conditional
	{
		/// <summary>
		/// Options for configuring the behavior of a ConditionalAttribute
		/// </summary>
		[Flags]
		public enum Options
		{
			None = 0,
			/// <summary>
			/// If set, the field will be hidden if the target matches the expected value,
			/// rather than if it doesn't match
			/// </summary>
			Invert = 1 << 0,
			/// <summary>
			/// If set, the field will be rendered as a read-only disabled field rather
			/// than completely hidden when its conditions are not met
			/// </summary>
			ShowDisabled = 1 << 1,
			/// <summary>
			/// If set, the field will be hidden if the target is also hidden
			/// </summary>
			Chain = 1 << 2,
		}
	}
}