using Xunit;
using static LeanCode.ContractsGenerator.Tests.Dart.ContractsGeneratorTestHelpers;

namespace LeanCode.ContractsGenerator.Tests.Dart
{
    public partial class ContractsGeneratorTests
    {
        [Fact]
        public void Private_class_is_not_resolved()
        {
            var generator = CreateDartGeneratorFromNamespace("private class TestClass { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.DoesNotContain("TestClass", contracts);
        }

        [Fact]
        public void Protected_class_is_not_resolved()
        {
            var generator = CreateDartGeneratorFromNamespace("private class TestClass { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.DoesNotContain("TestClass", contracts);
        }

        [Fact]
        public void Class_with_no_access_modifier_is_not_resolved()
        {
            var generator = CreateDartGeneratorFromNamespace("class TestClass { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.DoesNotContain("TestClass", contracts);
        }

        [Fact]
        public void Public_class_is_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromNamespace("public class TestClass { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("class TestClass", contracts);
        }

        [Fact]
        public void Generic_class_is_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromNamespace("public class TestClass<T> { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("class TestClass<T>", contracts);
        }

        [Fact]
        public void Generic_class_with_constraints_is_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromNamespace("public interface IInt { } public class TestClass<T> where T: IList<IInt> { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("class TestClass<T implements List<IInt>", contracts);
        }

        [Fact]
        public void Class_inheritance_is_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromNamespace("public interface IInt { } public class TestClass : IInt {}");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("class TestClass extends IInt", contracts);
        }

        [Fact]
        public void Generic_class_inheritance_is_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromNamespace("public interface IInt<T> { } public class TestClass<T> : IInt<T> {}");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("class TestClass<T> extends IInt<T>", contracts);
        }

        [Fact]
        public void Deep_inheritance_is_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromNamespace("public interface IInt<T> { } public class TestClass : IInt<List<int>> {}");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("class TestClass extends IInt<List<int>>", contracts);
        }

        [Fact]
        public void Base_type_properties_are_mapped_correctly_to_and_from_json()
        {
            var generator = CreateDartGeneratorFromNamespace(
@"public class BaseTestClass
{
    public int BaseField { get; set; }
}
public class TestClass : BaseTestClass
{
    public string Field { get; set; }
}");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            var expectedBaseClassProjection =
@"class BaseTestClass {

    int baseField;

    @virtual
    Map<String, dynamic> toJsonMap() {
        final map = <String, dynamic>{
            'BaseField': baseField,
        };
        return map;
    }

    static BaseTestClass fromJson(Map map) => BaseTestClass()
            ..baseField = map['BaseField'];
}";
            Assert.Contains(expectedBaseClassProjection, contracts);

            var expectedClassProjection =
@"class TestClass extends BaseTestClass {

    String field;

    @override
    Map<String, dynamic> toJsonMap() {
        final map = <String, dynamic>{
            'Field': field,
        };
        map.addAll(super.toJsonMap());
        return map;
    }

    static TestClass fromJson(Map map) => TestClass()
            ..field = map['Field']
            ..baseField = map['BaseField'];
}";
            Assert.Contains(expectedClassProjection, contracts);
        }

        [Fact]
        public void Deep_generic_inheritance_is_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromNamespace("public interface IInt<T> { } public class TestClass<T> : IInt<List<T>> { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("class TestClass<T> extends IInt<List<T>>", contracts);
        }

        [Fact]
        public void Static_class_is_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromNamespace("public static class ErrorCodes { public const int Invalid = 1; }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("Invalid = 1", contracts);
            Assert.Contains("ErrorCodes {", contracts);
        }

        [Fact]
        public void Order_of_classes_is_alphanumeric()
        {
            var generator = CreateDartGeneratorFromNamespace("public class TestClass2 { } public class TestClass1 { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("class TestClass1", contracts);
            Assert.Contains("class TestClass2", contracts);

            var test1Index = contracts.IndexOf("class TestClass1");
            var test2Index = contracts.IndexOf("class TestClass2");

            Assert.True(test1Index < test2Index);
        }

        [Fact]
        public void Classes_having_the_same_names_are_supplied_with_minimal_namespace_name()
        {
            var generator = CreateDartGenerator(
@"namespace Aa.Bb.Cc
{
    public class Class
    { }
}
namespace Aaa.Bbb.Cc
{
    public class Class
    { }
}");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            var firstClass =
@"class CcBbClass {


    @virtual
    Map<String, dynamic> toJsonMap() {
        final map = <String, dynamic>{
        };
        return map;
    }

    static CcBbClass fromJson(Map map) => CcBbClass();
}";
            Assert.Contains(firstClass, contracts);

            var secondClass =
@"class CcBbbClass {


    @virtual
    Map<String, dynamic> toJsonMap() {
        final map = <String, dynamic>{
        };
        return map;
    }

    static CcBbbClass fromJson(Map map) => CcBbbClass();
}";
            Assert.Contains(secondClass, contracts);
        }
    }
}
