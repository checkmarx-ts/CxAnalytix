using CxAnalytix.Configuration;
using System;
using System.Linq;
using Xunit;

namespace Test.Configuration
{
	public class SecureConfigSectionAttributeTests
	{

		class BaseProps
		{
			public String prop1 { get; set; }
			public String prop2 { get; set; }

		}

		[SecureConfigSectionAttribute]
		class NoPropsSpecified : BaseProps
		{
		}

		[SecureConfigSectionAttribute(SensitiveStringProp = "prop2") ]
		class OnePropSpecified : BaseProps
		{
		}

		[SecureConfigSectionAttribute(SensitiveStringProp = "prop2")]
		[SecureConfigSectionAttribute(SensitiveStringProp = "prop1")]
		class TwoPropSpecified : BaseProps
		{
		}


		private static SecureConfigSectionAttribute[] GetAttribs (Type t)
		{
			return t.GetCustomAttributes(typeof(SecureConfigSectionAttribute), true) as SecureConfigSectionAttribute[];
		}


		[Fact]
		public void AssumePropIsSetWhenNoPropDefinedOnEmptyNonNullObject ()
		{
			var list = GetAttribs(typeof(NoPropsSpecified));

			var inst = new NoPropsSpecified();

			Assert.True(list.All((x) => x.IsPropSet(typeof(NoPropsSpecified), inst)));
		}

		[Fact]
		public void AssumePropIsSetWhenNoPropDefinedOnPopulatedNonNullObject()
		{
			var list = GetAttribs(typeof(NoPropsSpecified));

			var inst = new NoPropsSpecified { prop1 = "foo", prop2 = "bar" };

			Assert.True(list.All((x) => x.IsPropSet(typeof(NoPropsSpecified), inst)));
		}

		[Fact]
		public void AssumePropIsNOTSetWhenNoPropDefinedOnNullObject()
		{
			var list = GetAttribs(typeof(NoPropsSpecified));

			Assert.False(list.All((x) => x.IsPropSet(typeof(NoPropsSpecified), null) ));
		}


		[Fact]
		public void ExpectFalseWhenSpecifiedPropNotSetWhenSinglePropSpecified()
		{
			var list = GetAttribs(typeof(OnePropSpecified));

			var inst = new OnePropSpecified { prop1 = "foo"};

			Assert.False (list.All((x) => x.IsPropSet(typeof(OnePropSpecified), inst)));
		}


		[Fact]
		public void ExpectFalseWhenNoPropsSetWhenSinglePropSpecified()
		{
			var list = GetAttribs(typeof(OnePropSpecified));

			var inst = new OnePropSpecified {};

			Assert.False (list.All((x) => x.IsPropSet(typeof(OnePropSpecified), inst)));
		}

		[Fact]
		public void ExpectTrueWhenOnlySpecifiedPropsSetWhenSinglePropSpecified()
		{
			var list = GetAttribs(typeof(OnePropSpecified));

			var inst = new OnePropSpecified { prop2 = "foo" };

			Assert.True (list.All((x) => x.IsPropSet(typeof(OnePropSpecified), inst)));
		}

		[Fact]
		public void ExpectTrueWhenOtherPropsSetWhenSinglePropSpecified()
		{
			var list = GetAttribs(typeof(OnePropSpecified));

			var inst = new OnePropSpecified { prop2 = "foo", prop1 = "bar" };

			Assert.True (list.All((x) => x.IsPropSet(typeof(OnePropSpecified), inst)));
		}


		[Fact]
		public void ExpectTrueWhenAllPropsSetWhenMultiplePropSpecified()
		{
			var list = GetAttribs(typeof(TwoPropSpecified));

			var inst = new TwoPropSpecified { prop2 = "foo", prop1 = "bar" };

			Assert.True (list.All((x) => x.IsPropSet(typeof(TwoPropSpecified), inst)));
		}

		[Fact]
		public void ExpectFalseWhenOnePropSetWhenMultiplePropSpecified()
		{
			var list = GetAttribs(typeof(TwoPropSpecified));

			var inst = new TwoPropSpecified {prop1 = "bar" };

			Assert.False (list.All((x) => x.IsPropSet(typeof(TwoPropSpecified), inst)));

		}

		[Fact]
		public void ExpectFalseWhenNoPropsSetWhenMultiplePropSpecified()
		{
			var list = GetAttribs(typeof(TwoPropSpecified));

			var inst = new TwoPropSpecified ();

			Assert.False (list.All((x) => x.IsPropSet(typeof(TwoPropSpecified), inst)));
		}
	}
}
