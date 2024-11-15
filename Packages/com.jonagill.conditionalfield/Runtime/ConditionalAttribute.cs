using System;
using UnityEngine;

namespace ConditionalField 
{
	/// <summary>
	/// Attribute that marks a serialized field to only display in the inspector
	/// if the given target field/property/method has the given expected value.
	/// </summary>
	public class ConditionalAttribute : PropertyAttribute
	{
		public const Conditional.Options DefaultOptions = Conditional.Options.Chain;
		
		public readonly string targetName;
		public readonly bool hasExpectedValue;
		public readonly object[] expectedValues;
		public readonly Conditional.Options options;

		/// <summary>
		/// Marks a serialized field to only be displayed if the target field/property/method returns true
		/// </summary>
		public ConditionalAttribute(string targetName, Conditional.Options options = DefaultOptions)
		{
			this.targetName = targetName;
			hasExpectedValue = false;
			expectedValues = new object[] { true };
			this.options = options;
		}

		/// <summary>
		/// Marks a serialized field to only be displayed if the target field/property/method
		/// returns the expected value
		/// </summary>
		public ConditionalAttribute(string targetName, object expectedValue, Conditional.Options options = DefaultOptions)
		{
			this.targetName = targetName;
			this.hasExpectedValue = true;
			this.expectedValues = new object[] { expectedValue };
			this.options = options;
		}
		
		/// <summary>
		/// Marks a serialized field to only be displayed if the target field/property/method
		/// returns one of the expected values
		/// </summary>
		public ConditionalAttribute(string targetName, Conditional.Options options = DefaultOptions, params object[] expectedValues)
		{
			this.targetName = targetName;
			this.hasExpectedValue = true;
			this.expectedValues = expectedValues;
			this.options = options;
		}
	}
}