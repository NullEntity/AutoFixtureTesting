using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;

namespace AutoFixtureTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var fixture = new Fixture();
            fixture.Customize(new CompositeCustomization(
                new FooComponent(),
                new BarComponent()
            ));
            var baz = fixture.Create<Baz>();

            Assert.IsNull(baz.FooString, "Foo.FooString should be null");
            Assert.AreEqual(baz.BarString, "bar", "Bar.BarString should be constant \"bar\"");
            Assert.IsNotNull(baz.BazString, "Baz.BazString should be randomly generated");
        }
    }

    public class FooComponent : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<Foo>(composer => composer.OmitAutoProperties());
        }
    }

    public class BarComponent : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<Bar>(composer => composer.With(bar => bar.BarString, "bar"));
        }
    }

    public class Foo
    {
        public string FooString;
    }

    public class Bar : Foo
    {
        public string BarString;
    }

    public class Baz : Bar
    {
        public string BazString;
    }
}
