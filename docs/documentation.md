# `Wkg.EntityFrameworkCore` Documentation

`Wkg.EntityFrameworkCore` is a library that provides reflective entity configuration and procedure mapping for Entity Framework Core, as well as other re-usable components not directly related to [RECAP](./RECAP-concept.pdf).

- [`Wkg.EntityFrameworkCore` Documentation](#wkgentityframeworkcore-documentation)
  - [Getting Started](#getting-started)
    - [Requirements](#requirements)
    - [Installation](#installation)
  - [Usage](#usage)
    - [Entity Configuration](#entity-configuration)
      - [Mapping Entities with RECAP](#mapping-entities-with-recap)
        - [Entity Definition](#entity-definition)
        - [Entity Mapping](#entity-mapping)
      - [Entity Discovery](#entity-discovery)
        - [Via Reflection](#via-reflection)
        - [Using Manual Registration](#using-manual-registration)
      - [Configuring Inheritance Hierarchies](#configuring-inheritance-hierarchies)
        - [Table Per Hierarchy (TPH)](#table-per-hierarchy-tph)
        - [Table Per Type (TPT)](#table-per-type-tpt)
        - [Table Per Concrete Type (TPC)](#table-per-concrete-type-tpc)
      - [Configuring Many-to-Many Relationships](#configuring-many-to-many-relationships)
      - [Defining Mapping and Naming Policies](#defining-mapping-and-naming-policies)
    - [Stored Procedure Mapping](#stored-procedure-mapping)

## Getting Started

### Requirements

Depending on the intended use, the following requirements apply:

- **Only using the entity configuration features of RECAP:**
  - A .NET runtime matching a major version of the `Wkg.EntityFrameworkCore` package.
  - A compatible Entity Framework Core version and database provider.
- **Using the procedure mapping features of RECAP:**
  - The requirements listed above.
  - The RECAP database provider package corresponding to the database provider used by the application. Currently supported providers are:
    - Oracle: [Wkg.EntityFrameworkCore.Oracle](https://git.wkg.lan/WKG/components/wkg-entity-framework-core-oracle)
    - MySQL: [Wkg.EntityFrameworkCore.MySQL](https://git.wkg.lan/WKG/components/wkg-entity-framework-core-mysql)

### Installation

Follow the installation instructions provided in the [README](../README.md).

## Usage

### Entity Configuration

The Entity Configuration feature of RECAP acts as a lightweight wrapper around Entity Framework Core and allows developers to configure entity mappings in a more concise and maintainable way than the default EF Core configuration syntax.

RECAPs entity mapping philosophy is based on the following principles:

- **Database first:** Entity mappings should follow the database-first approach to reduce the risk of unclear or inefficient auto-generated database schemas.
- **Explicit is better than implicit:** Entity mappings should be defined explicitly, rather than inferred from naming conventions. This results in clearer and more maintainable code, and reduces the risk of unexpected behaviour.
- **Separation of concerns:** Entity mappings should be defined in a separate file, rather than in the entity class itself. This allows for cleaner and less cluttered entity classes, and improves readability. As such, RECAP incentivizes the use of the [Fluent API](https://docs.microsoft.com/en-us/ef/core/modeling/), rather than data annotations.
- **Proximity:** Entity mappings should be defined in close proximity to the entity class itself. This allows for easy navigation between the entity class and its mapping configuration, and improves readability. As such, RECAP incentivizes the use of partial classes to define entity mappings.
- **Modularity:** Entity mappings should be defined in a modular way, rather than in a single monolithic configuration file. This allows for easier navigation and reduces the risk of merge conflicts. As such, RECAP dictates the use of multiple partial classes to define entity mappings.

#### Mapping Entities with RECAP

In order to use the Reflective Entity Configuration feature of RECAP, the following steps must be taken.

##### Entity Definition

First, create a partial class for the entity to be configured, and define the properties of the entity as usual. You may also define navigation properties, or other properties that are not mapped to the database. Business logic directly related to the entity may also be defined here.

The following example shows a simple entity definition for a `Person` entity in the `Person.cs` file:

```csharp
public partial class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public int? ParentId { get; set; }

    public virtual Person? Parent { get; set; }
    public virtual List<Person> Children { get; set; }

    public bool HasChildren => Children.Count > 0;

    public string GetGreeting() 
    {
        StringBuilder bobTheBuilder = new($"Hello, my name is {Name}! I am ");
        _ = Age switch
        {
            < 0 => bobTheBuilder.Append("not born yet."),
            < 12 => bobTheBuilder.Append("just a kid."),
            < 18 => bobTheBuilder.Append("a teenager."),
            < 30 => bobTheBuilder.Append("a young adult."),
            _ => bobTheBuilder.Append("pretty old.")
        };
        return bobTheBuilder.ToString();
    }
}
```

##### Entity Mapping

Next, create a second file for the partial class that will contain the entity mapping configuration. The name of the file is not important, but it is recommended to use a name that clearly indicates that the file contains entity mapping configuration. In this example, the file is named `Person.mapping.cs`, which clearly states the purpose of the file, and also benefits from Visual Studio's file nesting feature.

In the mapping file, create a partial class for the entity, and inherit from `IReflectiveModelConfiguration<TEntity>` or `IModelConfiguration<TEntity>`, depending on whether or not you want to use the reflective entity discovery feature of RECAP. The following example shows a simple entity mapping configuration for the `Person` entity in the `Person.mapping.cs` file:

```csharp
public partial class Person : IReflectiveModelConfiguration<Person>
{
    public static void Configure(EntityTypeBuilder<Person> self)
    {
        self.ToTable("person")
            .HasKey(p => p.Id)
            .HasName("id");

        self.Property(my => my.Id)
            .HasColumnName("id")
            .HasColumnType("int")
            .IsRequired();
            
        self.Property(my => my.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .IsRequired();

        self.Property(my => my.Age)
            .HasColumnName("age")
            .HasColumnType("int")
            .IsRequired();
        
        self.Property(my => my.ParentId)
            .HasColumnName("parent_id")
            .HasColumnType("int");
        
        self.HasOne(my => my.Parent)
            .WithMany(parent => parent.Children)
            .HasForeignKey(my => my.ParentId)
            .HasConstraintName("fk_person_parent_id")
            .OnDelete(DeleteBehavior.SetNull);

        self.Ignore(my => my.HasChildren);
    }
}
```

The static `Configure` method is dictated by the `IReflectiveModelConfiguration<TEntity>` interface, and is invoked by RECAP when the entity is configured. The method takes an EF Core `EntityTypeBuilder<TEntity>` as its only parameter, which is used to configure the entity. The method is static to separate the configuration logic from the entity instance, and to allow for easy navigation between the entity class and its mapping configuration.

#### Entity Discovery

RECAP provides Reflective Entity Discovery, which allows developers to automatically discover and configure entities in a given assembly. This feature only applies to classes implementing `IReflectiveModelConfiguration<TEntity>`. Entities that do not implement this interface must be configured manually.

##### Via Reflection

Reflective Entity Discovery can be invoked via reflection, by calling the `LoadReflectiveModels()` extension method on the `ModelBuilder` instance in the `OnModelCreating` method of the `DbContext` class. The following example shows how to invoke Reflective Entity Discovery via reflection:

```csharp
public class MyDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.LoadReflectiveModels();
    }
}
```

##### Using Manual Registration

Alternatively, entities can be configured manually by calling the `LoadModel<TEntity>()` extension method on the `ModelBuilder` instance in the `OnModelCreating` method of the `DbContext` class for each entity that needs to be configured. The following example shows how to configure the `Person` entity manually:

```csharp
public class MyDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.LoadModel<Person>();
    }
}
```

> :warning:
> In order to reduce the risk of missing entities, it is recommended to use Reflective Entity Discovery, rather than manual registration.

#### Configuring Inheritance Hierarchies

RECAP supports all inheritance strategies supported by EF Core, including Table Per Hierarchy (TPH), Table Per Type (TPT), and Table Per Concrete Type (TPC). The following examples aim to demonstrate how to configure each of these inheritance strategies.

##### Table Per Hierarchy (TPH)

Table Per Hierarchy (TPH) is the default inheritance strategy used by EF Core. In this strategy, all entities in the inheritance hierarchy are mapped to a single table, and a discriminator column is used to differentiate between the different entity types. 

In RECAP, TPH inheritance is achieved by configuring the discriminator column in the base entity mapping, and by mapping any additional properties in the derived entity mappings. The following example shows how to configure TPH inheritance for the `Person`, `Child`, and `Adult` entities:

<details>
<summary style="font-style: italic">Show/hide <code>Person</code> configuration</summary>

`Person.cs` file:

```csharp
public abstract partial class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Discriminator { get; set; }
}
```

`Person.mapping.cs` file:

```csharp
public abstract partial class Person : IReflectiveModelConfiguration<Person>
{
    public static void Configure(EntityTypeBuilder<Person> self)
    {
        self.ToTable("person")
            .HasKey(p => p.Id)
            .HasName("id");

        self.Property(my => my.Id)
            .HasColumnName("id")
            .HasColumnType("int")
            .IsRequired();
            
        self.Property(my => my.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .IsRequired();

        self.Property(my => my.Discriminator)
            .HasColumnName("discriminator")
            .HasColumnType("varchar(255)")
            .IsRequired();
        
        self.HasDiscriminator(my => my.Discriminator)
            .HasValue<Child>("child")
            .HasValue<Adult>("adult");
    }
}
```

</details>
<details>
<summary style="font-style: italic">Show/hide <code>Adult</code> configuration</summary>

`Adult.cs` file:

```csharp
public partial class Adult : Person
{
    public int? ParentId { get; set; }

    public virtual Person? Parent { get; set; }
    public virtual List<Person> Children { get; set; }

    public bool HasChildren => Children.Count > 0;
}
```

`Adult.mapping.cs` file:

```csharp
public partial class Adult : Person, IReflectiveModelConfiguration<Adult>
{
    public static void Configure(EntityTypeBuilder<Adult> self)
    {
        self.Property(my => my.ParentId)
            .HasColumnName("parent_id")
            .HasColumnType("int");
        
        self.HasOne(my => my.Parent)
            .WithMany(parent => parent.Children)
            .HasForeignKey(my => my.ParentId)
            .HasConstraintName("fk_person_parent_id")
            .OnDelete(DeleteBehavior.SetNull);
        
        self.Ignore(my => my.HasChildren);
    }
}
```

</details>
<details>
<summary style="font-style: italic">Show/hide <code>Child</code> configuration</summary>

`Child.cs` file:

```csharp
public class Child : Person
{
}
```

In this example, the `Child` entity does not have any additional properties, so no additional mapping is required.

</details>

The imporant thing to note in this example is that the child entities automatically inherit the mapping of the base entity, so only the additional properties need to be mapped in the child entity configuration. This includes the table name, meaning that all entities in the inheritance hierarchy are mapped to the same table.

##### Table Per Type (TPT)

TODO

##### Table Per Concrete Type (TPC)

TODO

#### Configuring Many-to-Many Relationships

TODO

#### Defining Mapping and Naming Policies

TODO

### Stored Procedure Mapping

TODO