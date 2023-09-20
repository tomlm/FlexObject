// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;

namespace Flex.Tests
{
    [TestClass]
    [TestCategory("Flex Objects")]
    public class FlexObjectTests
    {
        [TestMethod]
        public void FlexObject_DynamicProperties()
        {
            var testValue = "testValue";
            var flex = new FlexObject();
            flex["test"] = testValue;
            Assert.AreEqual("testValue", flex["test"]);
            dynamic flex2 = flex;
            Assert.AreEqual("testValue", flex2.test);
            Assert.AreEqual("testValue", flex2["test"]);
            flex2.test2 = "foo";
            Assert.AreEqual("foo", flex2.test2);
            Assert.AreEqual("foo", flex2["test2"]);
        }

        class TestObject : FlexObject
        {
            public string Name { get; set; }
        }

        class TestBinder : GetMemberBinder
        {
            public TestBinder(string name, bool ignoreCase) : base(name, ignoreCase)
            {
            }

            public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void FlexObject_Contains()
        {
            var testValue = "testValue";
            var test = new TestObject();
            dynamic testObj = test;
            testObj.Name = testValue;
            testObj.Test = testValue;

            Assert.IsTrue(test.ContainsKey("Name"));
            Assert.IsTrue(test.ContainsKey("Test"));
            Assert.IsFalse(test.ContainsKey("foo"));
            Assert.IsTrue(testObj.ContainsKey("Name"));
            Assert.IsTrue(testObj.ContainsKey("Test"));
            Assert.IsFalse(testObj.ContainsKey("foo"));
        }

        [TestMethod]
        public void FlexObject_Clear()
        {
            var testValue = "testValue";
            var test = new TestObject();
            dynamic testObj = test;
            testObj.Name = testValue;
            testObj.Test = testValue;

            Assert.AreEqual(testValue, test.Name);
            Assert.AreEqual(testValue, test["Test"]);
            Assert.AreEqual(testValue, testObj.Name);
            Assert.AreEqual(testValue, testObj["Test"]);
            Assert.AreEqual(testValue, testObj.Test);
            test.Clear();
            Assert.IsNull(test.Name);
            Assert.IsNull(test["Test"]);
            Assert.IsNull(testObj.Name);
            Assert.IsNull(testObj.Test);
            Assert.IsNull(testObj["Test"]);
        }


        [TestMethod]
        public void FlexObject_AccessDeclaredProperties()
        {
            var testValue = "testValue";
            var test = new TestObject();
            dynamic testObj = test;
            testObj.Name = testValue;
            testObj.Test = testValue;

            Assert.AreEqual(testObj.Name, testObj.Test);

            object value;
            Assert.IsTrue(test.TryGetMember(new TestBinder("Name", false), out value));
            Assert.AreEqual(testValue, value as string);
            Assert.IsTrue(test.TryGetValue("Name", out value));
            Assert.AreEqual(testValue, value as string);
            test.Remove("Name");
            Assert.AreEqual(null, test.Name);
            Assert.AreEqual(testValue, testObj.Test);
            test.Remove("Test");
            Assert.AreEqual(null, testObj.Test);
        }


        [TestMethod]
        public void FlexObject_DynamicPropertyAccess()
        {
            dynamic testObj = new TestObject();
            testObj.Test = "test";

            Assert.AreEqual("test", testObj.Test);
        }

        [TestMethod]
        public void FlexObject_PropertyAccess()
        {
            var testObj = new TestObject();
            testObj.Name = "name";

            Assert.AreEqual("name", testObj.Name);
        }


        [TestMethod]
        public void FlexObject_IndexPropertyAccess()
        {
            var testObj = new TestObject();
            testObj["Name"] = "name";

            Assert.AreEqual("name", testObj["Name"]);
        }

        [TestMethod]
        public void FlexObject_DynamicIndexPropertyAccess()
        {
            dynamic testObj = new TestObject();
            testObj["Name"] = "name";

            Assert.AreEqual("name", testObj["Name"]);
        }

        [TestMethod]
        public void FlexObject_SerializeDynamic()
        {
            dynamic testObj = new TestObject();
            testObj.Name = "name";
            testObj.Test = "test";

            var testObj2 = new TestObject();
            testObj2.Name = "name";
            testObj2["Test"] = "test";

            var json = JsonConvert.SerializeObject(testObj);
            var json2 = JsonConvert.SerializeObject(testObj2);
            Assert.AreEqual(json, json2, "expect dynamic and typed serialization to be the same");

            var testObj3 = JsonConvert.DeserializeObject<TestObject>(json);
            var testObj4 = JsonConvert.DeserializeObject<TestObject>(json2);
            Assert.AreEqual(testObj3.Name, testObj.Name, "typed should roundtrip");
            Assert.AreEqual(testObj3["Test"], testObj["Test"], "indexed should roundtrip");
            Assert.AreEqual(((dynamic)testObj3).Test, ((dynamic)testObj).Test, "indexed should roundtrip");

            Assert.AreEqual(testObj4.Name, testObj2.Name, "typed should roundtrip");
            Assert.AreEqual(testObj4["Test"], testObj2["Test"], "indexed should roundtrip");
            Assert.AreEqual(((dynamic)testObj4).Test, ((dynamic)testObj2).Test, "dynamic should roundtrip");
        }

        [TestMethod]
        public void FlexObject_JsonSimpleSeralizeFormat()
        {
            var flex = new FlexObject();
            flex["test"] = "testProperty";

            var json = JsonConvert.SerializeObject(flex);


            string targetJson = "{'test':'testProperty'}";
            JObject target = JObject.Parse(targetJson);
            JObject fromFlexObject = JObject.Parse(json);

            bool areSame = JToken.DeepEquals(target, fromFlexObject);
            Assert.IsTrue(areSame, "Json documents did not match");
        }

        [TestMethod]
        public void FlexObject_JsonNestedDynamicSeralizeFormat()
        {
            var parent = new FlexObject();
            parent["test"] = "testProperty";

            var child = new FlexObject();
            child["prop1"] = "property1";

            parent["nested"] = child;

            var parentJson = JsonConvert.SerializeObject(parent);

            string targetJson = @"
                {
                    'test' : 'testProperty',
                    'nested' : {
                        'prop1' : 'property1'
                    }
                }";

            JObject target = JObject.Parse(targetJson);
            JObject fromFlexObject = JObject.Parse(parentJson);

            bool areSame = JToken.DeepEquals(target, fromFlexObject);
            Assert.IsTrue(areSame, "Json documents did not match");
        }

        public class Nested
        {
            public string Property1 { get; set; } = "one";
        }

        [TestMethod]
        public void FlexObject_JsonConcreteSeralizeFormat()
        {

            var parent = new FlexObject();
            parent["test"] = "testProperty";
            parent["nested"] = new Nested();

            var parentJson = JsonConvert.SerializeObject(parent);

            string correctJson = @"
                {
                    'test' : 'testProperty',
                    'nested' : {
                        'Property1' : 'one'
                    }
                }";

            JObject target = JObject.Parse(correctJson);
            JObject fromFlexObject = JObject.Parse(parentJson);

            bool areSame = JToken.DeepEquals(target, fromFlexObject);
            Assert.IsTrue(areSame, "Json documents did not match");
        }


        public class NestedFlex : FlexObject
        {
            public string Property1 { get; set; } = "one";
        }

        [TestMethod]
        public void FlexObject_JsonMixedSeralizeFormat()
        {
            var parent = new NestedFlex();
            parent["test"] = "testProperty";

            var parentJson = JsonConvert.SerializeObject(parent);

            string correctJson = @"
                {
                    'Property1' : 'one',
                    'test' : 'testProperty' 
                }";

            JObject target = JObject.Parse(correctJson);
            JObject fromFlexObject = JObject.Parse(parentJson);

            bool areSame = JToken.DeepEquals(target, fromFlexObject);
            Assert.IsTrue(areSame, "Json documents did not match");
        }

        [TestMethod]
        public void FlexObject_EnumProperties()
        {
            var test = new TestObject();
            test["item"] = "item";
            var props = test.GetProperties().ToList();
            Assert.IsTrue(props.Contains("item"));
            Assert.IsTrue(props.Contains("Name"));
        }

        public class Car :FlexObject
        {
            private string _name;
            public string Name { get => _name; set { _name = value; NotifyChanged(); } }
        }

        [TestMethod]
        public void FlexObject_ChangeNotification()
        {
            var car = new Car ();
            HashSet<string> notifications = new HashSet<string>();
            car.PropertyChanged += (s, e) => notifications.Add(e.PropertyName);
            car.Name = "Volvo";
            car["test"] = "test";
            Assert.IsTrue(notifications.Contains("Name"));
            Assert.IsTrue(notifications.Contains("test"));
            notifications.Clear();
            car.Remove("Name");
            car.Remove("test");
            Assert.IsTrue(notifications.Contains("Name"));
            Assert.IsTrue(notifications.Contains("test"));
        }

        [TestMethod]
        public void FlexObject_GetValuePerumutations()
        {
            var car = new Car()
            {
                Name = "Volvo"
            };
            car["test"] = "test";

            Assert.AreEqual("Volvo", car.GetValue("Name"));
            Assert.AreEqual("Volvo", car.GetValue<string>("Name"));
            car.TryGetValue<string>("Name", out var result1);
            Assert.AreEqual("Volvo", result1);

            car.TryGetValue("Name", out var result2);
            Assert.AreEqual("Volvo", result2);
        }
    }
}