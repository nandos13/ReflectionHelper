using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using static JakePerry.Reflection.ReflectionUtility;

namespace JakePerry.Reflection.Tests
{
    [TestClass]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible")]
    public class ReflectionUtility_Tests
    {
        public class ClassA<T> { }
        public class ClassB<T> : ClassA<T> { }
        public class ClassC : ClassB<object> { }

        public interface IInterfaceA<T> { }
        public interface IInterfaceB<T> : IInterfaceA<T> { }
        public interface IInterfaceC : IInterfaceB<object> { }

        public class ImplementingClassA<T> : IInterfaceA<T> { }
        public class ImplementingClassB<T> : ImplementingClassA<T> { }
        public class ImplementingClassC : ImplementingClassB<object> { }

        [TestMethod("Invalid args: " + nameof(TypeDerivesFromClassOfGenericDefinition))]
        [Description("Tests whether the TypeDerivesFromClassOfGenericDefinition method returns false for invalid arguments.")]
        public void TestMethod_InvalidArgs_TypeDerivesFromClassOfGenericDefinition()
        {
            // Test null source type argument
            Assert.IsFalse(TypeDerivesFromClassOfGenericDefinition(null, typeof(ClassA<>)));

            // Test null generic type definition argument
            Assert.IsFalse(TypeDerivesFromClassOfGenericDefinition(typeof(ClassC), null));

            // Test all null arguments
            Assert.IsFalse(TypeDerivesFromClassOfGenericDefinition(null, null));

            // Test non-generic type definition argument for genericTypeDefinition parameter
            Assert.IsFalse(TypeDerivesFromClassOfGenericDefinition(typeof(ClassC), typeof(ClassC)));

            // Test non-class type for genericTypeDefinition parameter
            Assert.IsFalse(TypeDerivesFromClassOfGenericDefinition(typeof(ClassC), typeof(IInterfaceA<>)));
        }

        [TestMethod(nameof(TypeDerivesFromClassOfGenericDefinition))]
        public void TestMethod_TypeDerivesFromClassOfGenericDefinition()
        {
            Assert.IsTrue(TypeDerivesFromClassOfGenericDefinition(typeof(ClassA<>), typeof(ClassA<>)));
            Assert.IsTrue(TypeDerivesFromClassOfGenericDefinition(typeof(ClassA<object>), typeof(ClassA<>)));
            Assert.IsTrue(TypeDerivesFromClassOfGenericDefinition(typeof(ClassB<>), typeof(ClassA<>)));
            Assert.IsTrue(TypeDerivesFromClassOfGenericDefinition(typeof(ClassB<object>), typeof(ClassA<>)));
            Assert.IsTrue(TypeDerivesFromClassOfGenericDefinition(typeof(ClassC), typeof(ClassA<>)));
        }

        [TestMethod("Invalid args: " + nameof(TypeImplementsInterfaceOfGenericDefinition))]
        [Description("Tests whether the TypeImplementsInterfaceOfGenericDefinition method returns false for invalid arguments.")]
        public void TestMethod_InvalidArgs_TypeImplementsInterfaceOfGenericDefinition()
        {
            // Test null source type argument
            Assert.IsFalse(TypeImplementsInterfaceOfGenericDefinition(null, typeof(IInterfaceA<>)));

            // Test null generic type definition argument
            Assert.IsFalse(TypeImplementsInterfaceOfGenericDefinition(typeof(IInterfaceC), null));

            // Test all null arguments
            Assert.IsFalse(TypeImplementsInterfaceOfGenericDefinition(null, null));

            // Test non-generic type definition argument for genericTypeDefinition parameter
            Assert.IsFalse(TypeImplementsInterfaceOfGenericDefinition(typeof(IInterfaceC), typeof(IInterfaceC)));

            // Test non-interface type for genericTypeDefinition parameter
            Assert.IsFalse(TypeImplementsInterfaceOfGenericDefinition(typeof(IInterfaceC), typeof(ClassA<>)));
        }

        [TestMethod(nameof(TypeImplementsInterfaceOfGenericDefinition))]
        public void TestMethod_TypeImplementsInterfaceOfGenericDefinition()
        {
            // Test against interface types
            Assert.IsTrue(TypeImplementsInterfaceOfGenericDefinition(typeof(IInterfaceA<>), typeof(IInterfaceA<>)));
            Assert.IsTrue(TypeImplementsInterfaceOfGenericDefinition(typeof(IInterfaceA<object>), typeof(IInterfaceA<>)));
            Assert.IsTrue(TypeImplementsInterfaceOfGenericDefinition(typeof(IInterfaceB<>), typeof(IInterfaceA<>)));
            Assert.IsTrue(TypeImplementsInterfaceOfGenericDefinition(typeof(IInterfaceB<object>), typeof(IInterfaceA<>)));
            Assert.IsTrue(TypeImplementsInterfaceOfGenericDefinition(typeof(IInterfaceC), typeof(IInterfaceA<>)));

            // Test against implementing class types
            Assert.IsTrue(TypeImplementsInterfaceOfGenericDefinition(typeof(ImplementingClassA<>), typeof(IInterfaceA<>)));
            Assert.IsTrue(TypeImplementsInterfaceOfGenericDefinition(typeof(ImplementingClassA<object>), typeof(IInterfaceA<>)));
            Assert.IsTrue(TypeImplementsInterfaceOfGenericDefinition(typeof(ImplementingClassB<>), typeof(IInterfaceA<>)));
            Assert.IsTrue(TypeImplementsInterfaceOfGenericDefinition(typeof(ImplementingClassB<object>), typeof(IInterfaceA<>)));
            Assert.IsTrue(TypeImplementsInterfaceOfGenericDefinition(typeof(ImplementingClassC), typeof(IInterfaceA<>)));
        }
    }
}
