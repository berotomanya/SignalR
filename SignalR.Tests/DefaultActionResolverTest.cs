﻿using System.Collections.Generic;
using SignalR.Hubs;
using Xunit;

namespace SignalR.Tests {
    public class DefaultActionResolverTest {
        [Fact]
        public void ResolveActionExcludeHubMethods() {
            var resolver = new DefaultActionResolver();
            var actionInfo1 = resolver.ResolveAction(typeof(TestHub), "AddToGroup", new object[] { "admin" });
            var actionInfo2 = resolver.ResolveAction(typeof(TestHub), "RemoveFromGroup", new object[] { "admin" });

            Assert.Null(actionInfo1);
            Assert.Null(actionInfo2);
        }

        [Fact]
        public void ResolveActionOnDerivedHubFindsMethodOnBasedType() {
            var resolver = new DefaultActionResolver();
            var actionInfo = resolver.ResolveAction(typeof(TestDerivedHub), "Foo", new object[] { });

            Assert.NotNull(actionInfo);
            Assert.Equal("Foo", actionInfo.Method.Name);
            Assert.Equal(0, actionInfo.Arguments.Length);
        }

        [Fact]
        public void ResolveActionExcludesPropertiesOnDeclaredType() {
            var resolver = new DefaultActionResolver();
            var actionInfo = resolver.ResolveAction(typeof(TestHub), "get_Value", new object[] { });

            Assert.Null(actionInfo);
        }

        [Fact]
        public void ResolveActionExcludesPropetiesOnBaseTypes() {
            var resolver = new DefaultActionResolver();
            var actionInfo = resolver.ResolveAction(typeof(TestHub), "get_Clients", new object[] { });

            Assert.Null(actionInfo);
        }

        [Fact]
        public void ResolveActionLocatesPublicMethodsOnHub() {
            var resolver = new DefaultActionResolver();
            var actionInfo = resolver.ResolveAction(typeof(TestHub), "Foo", new object[] { });

            Assert.NotNull(actionInfo);
            Assert.Equal("Foo", actionInfo.Method.Name);
            Assert.Equal(0, actionInfo.Arguments.Length);
        }

        [Fact]
        public void ResolveActionReturnsNullIfMethodAmbiguous() {
            var resolver = new DefaultActionResolver();
            var actionInfo = resolver.ResolveAction(typeof(TestHub), "Bar", new object[] { 1 });

            Assert.Null(actionInfo);
        }

        [Fact]
        public void ResolveActionPicksMethodWithMatchingArguments() {
            var resolver = new DefaultActionResolver();
            var actionInfo = resolver.ResolveAction(typeof(TestHub), "Foo", new object[] { 1 });

            Assert.NotNull(actionInfo);
            Assert.Equal("Foo", actionInfo.Method.Name);
            Assert.Equal(1, actionInfo.Method.GetParameters().Length);
            Assert.Equal(1, actionInfo.Arguments.Length);
        }

        [Fact]
        public void ResolveActionBindsComplexArgumentsWithDictionary() {
            var resolver = new DefaultActionResolver();
            var arg = new Dictionary<string, object> {
                { "Age", 1 },
                { "Address",  new Dictionary<string, object> {
                                { "Street",  "The street" },
                                { "Zip", 34567 }
                              } 
                }
            };

            var actionInfo = resolver.ResolveAction(typeof(TestHub), "MethodWithComplex", new object[] { arg });

            Assert.NotNull(actionInfo);
            var complex = actionInfo.Arguments[0] as Complex;
            Assert.NotNull(complex);
            Assert.Equal(1, complex.Age);
            Assert.NotNull(complex.Address);
            Assert.Equal("The street", complex.Address.Street);
            Assert.Equal(34567, complex.Address.Zip);
        }

        [Fact]
        public void ResolveActionBindsSimpleArrayArgument() {
            var resolver = new DefaultActionResolver();

            var actionInfo = resolver.ResolveAction(typeof(TestHub),
                                                    "MethodWithArray",
                                                    new object[] { new object[] { 1, 2, 3 } });

            Assert.NotNull(actionInfo);
            var args = actionInfo.Arguments[0] as int[];
            Assert.Equal(1, args[0]);
            Assert.Equal(2, args[1]);
            Assert.Equal(3, args[2]);
        }

        [Fact]
        public void ResolveActionBindsComplexArrayArgument() {
            var resolver = new DefaultActionResolver();
            var arg = new Dictionary<string, object> {
                { "Age", 1 },
                { "Address",  new Dictionary<string, object> {
                                { "Street",  "The street" },
                                { "Zip", 34567 }
                              } 
                }
            };


            var actionInfo = resolver.ResolveAction(typeof(TestHub),
                                                    "MethodWithArrayOfComplete",
                                                    new object[] { new object[] { arg } });

            Assert.NotNull(actionInfo);
            var complexArray = actionInfo.Arguments[0] as Complex[];
            Assert.Equal(1, complexArray.Length);
            var complex = complexArray[0];
            Assert.NotNull(complex);
            Assert.Equal(1, complex.Age);
            Assert.NotNull(complex.Address);
            Assert.Equal("The street", complex.Address.Street);
            Assert.Equal(34567, complex.Address.Zip);
        }

        private class TestDerivedHub : TestHub {
            public void FooDerived() {
                Foo();
            }
        }

        private class TestHub : Hub {
            public int Value { get; set; }

            public void Foo() {
            }

            public void Foo(int x) {

            }

            public void Bar(double d) {

            }

            public void Bar(int x) {

            }

            public void MethodWithArray(int[] values) {

            }

            public void MethodWithArrayOfComplete(Complex[] complexes) {

            }

            public void MethodWithComplex(Complex complex) {

            }
        }

        public class Complex {
            public int Age { get; set; }
            public Address Address { get; set; }
        }

        public class Address {
            public string Street { get; set; }
            public int Zip { get; set; }
        }
    }
}
