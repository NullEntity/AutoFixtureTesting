using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using AutoFixture;
using AutoFixture.Kernel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoFixtureTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var fixture = new Fixture();
            fixture.Customizations.Add(new PropertyDeclaringTypeOmitter(typeof(Foo)));
            fixture.CustomizePostActions<Bar>(transformation => transformation.With(bar => bar.BarString, "bar"));
            var baz = fixture.Create<Baz>();

            Assert.IsNull(baz.FooString, "Foo.FooString should be null");
            Assert.AreEqual("bar", baz.BarString, "Bar.BarString should be constant \"bar\"");
            Assert.IsNotNull(baz.BazString, "Baz.BazString should be randomly generated");
        }
    }

    public static class PostActionsCustomization
    {
        public static IFixture CustomizePostActions<T>(
            this IFixture fixture,
            Func<PostActionTransformation<T>, ISpecimenBuilderTransformation> customizations)
        {
            fixture.Behaviors.Add(customizations.Invoke(new PostActionTransformation<T>()));

            return fixture;
        }

        public class PostActionTransformation<T> : ISpecimenBuilderTransformation
        {
            private readonly List<ISpecimenCommand> _commands = new List<ISpecimenCommand>();

            public ISpecimenBuilder Transform(ISpecimenBuilder builder)
            {
                return new Postprocessor(
                    builder,
                    new CompositeSpecimenCommand(_commands),
                    new IsAssignableToTypeSpecification(typeof(T)));
            }

            public PostActionTransformation<T> With<TProperty>(Expression<Func<T, TProperty>> propertyPicker,
                TProperty value)
            {
                _commands.Add(new BindingCommand<T, TProperty>(propertyPicker, value));

                return this;
            }
        }

        /// <summary>
        /// Specification that checks that request type is assignable to the specified type
        /// </summary>
        private class IsAssignableToTypeSpecification : IRequestSpecification
        {
            private readonly Type _expectedType;

            public IsAssignableToTypeSpecification(Type expectedType)
            {
                if (expectedType == null)
                    throw new ArgumentNullException(nameof(expectedType));
                _expectedType = expectedType;
            }

            public bool IsSatisfiedBy(object request)
            {
                var typeRequest = request as Type;
                return typeRequest != null && _expectedType.IsAssignableFrom(typeRequest);
            }
        }
    }

    internal class PropertyDeclaringTypeOmitter : ISpecimenBuilder
    {
        private readonly Type _type;

        internal PropertyDeclaringTypeOmitter(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            _type = type;
        }

        public object Create(object request, ISpecimenContext context)
        {
            var propInfo = request as PropertyInfo;
            if (propInfo != null && propInfo.DeclaringType == _type)
                return new OmitSpecimen();

            var fieldInfo = request as FieldInfo;
            if (fieldInfo != null && fieldInfo.DeclaringType == _type)
                return new OmitSpecimen();

            return new NoSpecimen();
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
