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
        - [Reflective Entity Discovery](#reflective-entity-discovery)
        - [Manual Entity Registration](#manual-entity-registration)
      - [Configuring Inheritance Hierarchies](#configuring-inheritance-hierarchies)
        - [Table Per Hierarchy (TPH)](#table-per-hierarchy-tph)
        - [Table Per Type (TPT)](#table-per-type-tpt)
        - [Table Per Concrete Type (TPC)](#table-per-concrete-type-tpc)
      - [Configuring Many-to-Many Relationships](#configuring-many-to-many-relationships)
        - [Entity Definitions](#entity-definitions)
        - [Connection Entity Definition](#connection-entity-definition)
        - [Connection Entity Discovery](#connection-entity-discovery)
          - [Reflective Connection Discovery](#reflective-connection-discovery)
          - [Manual Connection Registration](#manual-connection-registration)
      - [Enforcing Policies](#enforcing-policies)
        - [Naming Policies](#naming-policies)
        - [Mapping Policies](#mapping-policies)
        - [Inheritance Policies](#inheritance-policies)
        - [Custom Policies](#custom-policies)
          - [Example: Code-First Naming with Snake Case](#example-code-first-naming-with-snake-case)
          - [Example: Ensure All UUID Properties are Database-Generated](#example-ensure-all-uuid-properties-are-database-generated)
          - [Example: Automatically Ignoring Navigation Properties during JSON Serialization](#example-automatically-ignoring-navigation-properties-during-json-serialization)
    - [Stored Procedure Mapping](#stored-procedure-mapping)
      - [Getting Started with PCO Mapping](#getting-started-with-pco-mapping)
        - [Mapping a Database Function](#mapping-a-database-function)
        - [Mapping a Stored Procedure](#mapping-a-stored-procedure)
      - [PCO Discovery](#pco-discovery)
    - [Executing PCOs](#executing-pcos)

> :warning: **Warning**
> This documentation is a work in progress and may not be complete or up-to-date. For the most accurate and up-to-date information, please refer to the source code and the XML documentation comments.

## Getting Started

### Requirements

Depending on the intended use, the following requirements apply:

- **Only using the entity configuration features of RECAP:**
  - A .NET runtime matching a major version of the `Wkg.EntityFrameworkCore` package.
  - A compatible Entity Framework Core version and database provider.
- **Using the procedure mapping features of RECAP:**
  - The requirements listed above.
  - The RECAP database provider package corresponding to the database provider used by the application. Currently supported providers are:
    - Oracle: [Wkg.EntityFrameworkCore.Oracle](https://github.com/WKG-Software-GmbH/wkg-entity-framework-core-oracle)
    - MySQL: [Wkg.EntityFrameworkCore.MySQL](https://github.com/WKG-Software-GmbH/wkg-entity-framework-core-mysql)

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
            .HasKey(my => my.Id)
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

##### Reflective Entity Discovery

Entity Discovery can be performed automatically by RECAP via reflection. You can enable Reflective Entity Discovery by calling the `LoadReflectiveModels()` extension method on the `ModelBuilder` instance in the `OnModelCreating` method of the `DbContext` class. The following example shows how to invoke Reflective Entity Discovery via reflection:

```csharp
public class MyDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.LoadReflectiveModels();
    }
}
```

> :bulb: **Tip**
> If you are targeting multiple databases (i.e., have multiple `DbContext` classes), you can use the `LoadReflectiveModels<TDatabaseEngineModelAttribute>()` extension method to only load entities that are decorated with the specified database engine attribute.

##### Manual Entity Registration

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

> :warning: **Warning**
> In order to reduce the risk of missing entities, it is recommended to use [Reflective Entity Discovery](#reflective-entity-discovery), rather than manual registration.

#### Configuring Inheritance Hierarchies

RECAP supports all inheritance strategies supported by EF Core, including Table Per Hierarchy (TPH), Table Per Type (TPT), and Table Per Concrete Type (TPC). The following examples aim to demonstrate how to configure each of these inheritance strategies.

##### Table Per Hierarchy (TPH)

Table Per Hierarchy (TPH) is the default inheritance strategy used by EF Core. In this strategy, all entities in the inheritance hierarchy are mapped to a single table, and a discriminator column is used to differentiate between the different entity types. 

In RECAP, TPH inheritance is achieved by configuring the discriminator column in the base entity mapping, and by mapping any additional properties in the derived entity mappings. The following example shows how to configure TPH inheritance for the `Person`, `Child`, and `Adult` entities:

<details>
<summary>

*Show/hide <code>Person</code> TPH configuration*

</summary>

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
            .HasKey(my => my.Id)
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
<summary>

*Show/hide <code>Adult</code> TPH configuration*

</summary>

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
<summary>

*Show/hide <code>Child</code> TPH configuration*

</summary>

`Child.cs` file:

```csharp
public class Child : Person
{
}
```

In this example, the `Child` entity does not have any additional properties, so no additional mapping is required.

</details>

> :information_source: **Note**
> In TPH inheritance, the child entities automatically inherits the mapping of the base entity, so only the additional properties need to be mapped in the child entity configuration. This includes the table name, meaning that all entities in the inheritance hierarchy are mapped to the same table.

##### Table Per Type (TPT)

In Table Per Type (TPT) inheritance, each entity in the inheritance hierarchy is mapped to its own table. Parent entities are mapped to a table that contains all of the properties of the parent and child entities, every child entity is mapped to its own table that contains only the additional properties of that child entity. Child entities are then linked to their parent entities via a foreign key constraint. 

For easy linking, the primary key of the child entity is also the foreign key to the parent entity, meaning the ID of the child entity is the same as the ID of the parent entity.

The following example shows how to configure TPT inheritance for the `Person`, `Child`, and `Adult` entities:

<details>
<summary>

*Show/hide <code>Person</code> TPT configuration*

</summary>

`Person.cs` file:

```csharp
public abstract partial class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

`Person.mapping.cs` file:

```csharp
public abstract partial class Person : IReflectiveModelConfiguration<Person>
{
    public static void Configure(EntityTypeBuilder<Person> self)
    {
        self.ToTable("person")
            .HasKey(my => my.Id)
            .HasName("id");

        self.Property(my => my.Id)
            .HasColumnName("id")
            .HasColumnType("int")
            .IsRequired();
            
        self.Property(my => my.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .IsRequired();
    }
}
```

</details>
<details>
<summary>

*Show/hide <code>Adult</code> TPT configuration*

</summary>

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
        self.ToTable("adult");

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
<summary>

*Show/hide <code>Child</code> TPT configuration*

</summary>

`Child.cs` file:

```csharp
public partial class Child : Person
{
}
```

`Child.mapping.cs` file:

```csharp
public partial class Child : Person, IReflectiveModelConfiguration<Child>
{
    public static void Configure(EntityTypeBuilder<Child> self)
    {
        self.ToTable("child");
    }
}
```

</details>

> :information_source: **Note**
> In TPT inheritance, the child entities automatically inherits the property mapptings of the base entity, so only the additional properties need to be mapped in the child entity configuration. However, in order to map the child entity to its own table, the child entity configuration must override the table name mapping of the base entity.

Because all entities are mapped to their own respective tables, EF Core will automatically detect that TPT inheritance is being used and will generate the appropriate SQL `JOIN` statements on the `id` columns of the parent and child tables.

> :warning: **Warning**
> In many cases, TPT shows inferior performance when compared to TPH. [See the performance docs for more information](https://learn.microsoft.com/en-us/ef/core/performance/modeling-for-performance#inheritance-mapping).

##### Table Per Concrete Type (TPC)

In Table Per Concrete Type (TPC) inheritance, only **concrete** entities are mapped to tables. Abstract entities are not mapped to tables. This means that properties of abstract entities are duplicated in the tables of the concrete entities that inherit from them.

RECAP allows you to share the mapping configuration of inherited properties between the concrete entities that inherit from the same abstract entity. This is done by implementing `IBaseModelConfiguration<TParentClass>` or `IReflectiveBaseModelConfiguration<TParentClass>` in the abstract entity configuration, and configuring the concrete entities as usual with the `IReflectiveModelConfiguration<TChildClass>` interface.

> :warning: **Warning**
> If your are not using [Reflective Entity Discovery](#reflective-entity-discovery), you must manually call into the base entity configuration from the concrete entity configuration. See the [Entity Discovery](#entity-discovery) and [Configuring Many-to-Many Relationships](#configuring-many-to-many-relationships) sections for more information.

The following example shows how to configure TPC inheritance for the `Person`, `Child`, and `Adult` 
entities:

<details>
<summary>

*Show/hide <code>Person</code> TPC configuration*

</summary>

`Person.cs` file:

```csharp
public abstract partial class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

`Person.mapping.cs` file:

```csharp
public abstract partial class Person : IReflectiveBaseModelConfiguration<Person>
{
    static void IReflectiveBaseModelConfiguration<Person>.ConfigureBaseModel<TChildClass>(EntityTypeBuilder<TChildClass> self)
    {
        self.HasKey(my => my.Id)
            .HasName("id");

        self.Property(my => my.Id)
            .HasColumnName("id")
            .HasColumnType("int")
            .IsRequired();
            
        self.Property(my => my.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .IsRequired();
    }
}
```

> :information_source: **Note**
> Notice that the `Person` entity configuration implements `IReflectiveBaseModelConfiguration<Person>` instead of `IReflectiveModelConfiguration<Person>`, and that no table mapping is done in the `Person` entity configuration.

</details>
<details>
<summary>

*Show/hide <code>Adult</code> TPC configuration*

</summary>

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
        self.ToTable("adult");

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
<summary>

*Show/hide <code>Child</code> TPC configuration*

</summary>

`Child.cs` file:

```csharp
public partial class Child : Person
{
}
```

`Child.mapping.cs` file:

```csharp
public partial class Child : Person, IReflectiveModelConfiguration<Child>
{
    public static void Configure(EntityTypeBuilder<Child> self)
    {
        self.ToTable("child");
    }
}
```

</details>

> :warning: **Warning**
> Be sure to not implement the "normal" model configuration interface (`IModelConfiguration<TEntity>` or `IReflectiveModelConfiguration<TEntity>`) in the abstract entity configuration instead of the **base** model configuration interface (`IBaseModelConfiguration<TParentClass>` or `IReflectiveBaseModelConfiguration<TParentClass>`), as the base entity itself must not be configured as a model in TPC inheritance.

#### Configuring Many-to-Many Relationships

RECAP allows you to easily configure connection entities for many-to-many relationships. The following examples shows how to configure a many-to-many relationship between the `Person` and `Group` entities.

##### Entity Definitions

First, configure the `Person` and `Group` entities as usual:

<details>
<summary>

*Show/hide <code>Person</code> entity configuration*

</summary>

`Person.cs` file:

```csharp
public partial class Person
{
    public int Id { get; set; }
    public string Name { get; set; }

    public virtual List<Group> Groups { get; set; }
}
```

`Person.mapping.cs` file:

```csharp
public partial class Person : IReflectiveModelConfiguration<Person>
{
    public static void Configure(EntityTypeBuilder<Person> self)
    {
        self.ToTable("person")
            .HasKey(my => my.Id)
            .HasName("id");

        self.Property(my => my.Id)
            .HasColumnName("id")
            .HasColumnType("int")
            .IsRequired();
            
        self.Property(my => my.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .IsRequired();
    }
}
```

> :information_source: **Note**
> Notice that the `Person` entity does not configure the `Groups` navigation property. This is because the navigation property will be configured by the connection entity.

</details>
<details>
<summary>

*Show/hide <code>Group</code> entity configuration*

</summary>

`Group.cs` file:

```csharp
public partial class Group
{
    public int Id { get; set; }
    public string GroupName { get; set; }

    public virtual List<Person> Members { get; set; }
}
```

`Group.mapping.cs` file:

```csharp
public partial class Group : IReflectiveModelConfiguration<Group>
{
    public static void Configure(EntityTypeBuilder<Group> self)
    {
        self.ToTable("group")
            .HasKey(my => my.Id)
            .HasName("id");

        self.Property(my => my.Id)
            .HasColumnName("id")
            .HasColumnType("int")
            .IsRequired();
            
        self.Property(my => my.GroupName)
            .HasColumnName("group_name")
            .HasColumnType("varchar(255)")
            .IsRequired();
    }
}
```

> :information_source: **Note**
> Notice that the `Group` entity does not configure the `Members` navigation property. This is because the navigation property will be configured by the connection entity.

</details>

##### Connection Entity Definition

Next, create the connection entity by implementing either `IModelConnection<TConnection, TLeft, TRight>` or `IReflectiveModelConnection<TConnection, TLeft, TRight>`. The following example shows how to create a connection entity named `PersonToGroup`:

<details>
<summary>

*Show/hide <code>PersonToGroup</code> connection entity configuration*

</summary>

`PersonToGroup.cs` file:

```csharp
public partial class PersonToGroup
{
    public int PersonId { get; set; }
    public int GroupId { get; set; }

    public virtual Person Person { get; set; }
    public virtual Group Group { get; set; }
}
```

`PersonToGroup.mapping.cs` file:

```csharp
public partial class PersonToGroup : IReflectiveModelConnection<PersonToGroup, Person, Group>
{
    public static void ConfigureConnection(EntityTypeBuilder<PersonToGroup> self)
    {
        _ = self.ToTable("PersonToGroup")
            .HasKey(my => new
            {
                my.PersonId,
                my.GroupId
            });

        _ = self.Property(my => my.ContentId)
            .HasColumnName("content_id")
            .HasColumnType("int")
            .IsRequired();

        _ = self.Property(my => my.GenreId)
            .HasColumnName("genre_id")
            .HasColumnType("int")
            .IsRequired();
    }

    public static void Connect(EntityTypeBuilder<Person> left, EntityTypeBuilder<Group> right) => left
        .HasMany(person => person.Groups)
        .WithMany(group => group.Members)
        .UsingEntity<PersonToGroup>(
            self => self
                .HasOne(my => my.Person)
                .WithMany()
                .HasForeignKey(my => my.PersonId)
                .OnDelete(DeleteBehavior.Cascade),
            self => self
                .HasOne(my => my.Group)
                .WithMany()
                .HasForeignKey(my => my.GroupId)
                .OnDelete(DeleteBehavior.Cascade),
            ConfigureConnection);
}
```

As you can see in the example above, the connection entity implements the `IReflectiveModelConnection<TConnection, TLeft, TRight>` interface. This interface requires the implementation of the `ConfigureConnection` method, which configures the connection entity itself. The `Connect` method configures the many-to-many relationship between the `Person` and `Group` entities and passes the `ConfigureConnection` method as the configuration method for the connection entity to EF Core. This way, the connection entity is configured automatically when the `Person` and `Group` entities are configured.

> :information_source: **Note**
> Notice that the `PersonToGroup` connection entity configures the navigation properties of the `Person` and `Group` entities. This is because the connection entity is the only entity that knows the specifics of the many-to-many relationship between the `Person` and `Group` entities.

</details>

##### Connection Entity Discovery

Finally, you need to tell RECAP to discover the connection entity. Similarly to the [Entity Discovery](#entity-discovery), you can opt to let RECAP discover the connection entity automatically or choose to register the connection entities manually.

###### Reflective Connection Discovery

If you are using [Reflective Entity Discovery](#reflective-entity-discovery), RECAP will automatically load the connection entities as well, as long as they implement the `IReflectiveModelConnection<TConnection, TLeft, TRight>` interface. This is the recommended way of applying connection entity definitions.

###### Manual Connection Registration

You can also register the connection entities manually by calling the `LoadConnection<TConnection, TLeft, TRight>()` extension method on the `ModelBuilder` instance for each connection entity. The following example shows how to register the `PersonToGroup` connection entity:

```csharp
class MyDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        _ = modelBuilder
            .LoadModel<Person>()
            .LoadModel<Group>()
            .LoadConnection<PersonToGroup, Person, Group>();
    }
}
```

> :warning: **Warning**
> If you are using manual connection registration, be sure to register the connection entities after the entities they connect have been registered.

#### Enforcing Policies

RECAP allows you to enforce different policies for your Entity Framework mappings. Policies can be applied to all entities loaded via Reflective Entity Discovery, allowing you to enforce code quality standards, validate mappings to discover potential issues, or execute custom initialization logic for all discovered entities.

Policies are configured by calling the `ConfigurePolicies()` method on the `IModelOptionsBuilder` instance in the `LoadReflectiveModels` method. The following example shows how to apply built-in naming and mapping policies to all entities loaded via Reflective Entity Discovery:

```csharp
class MyDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.LoadReflectiveModels(options => options
            .ConfigurePolicies(policies => policies
                .AddPolicy<EntityNaming>(naming => naming.RequireExplicit())
                .AddPolicy<PropertyMapping>(properties => properties.IgnoreImplicit())));
    }
}
```

RECAP allows you to use predefined or custom mapping and naming policies to enforce different policies for your Entity Framework mappings. Following the principles of *"explicit is better than implicit"*, you can, for example, prohibit EF Core from applying convention-based mappings if no explicit name has been specified for an entity or property. This way, you can ensure that all mappings are explicitly defined and that renaming a property or class will not break your application.

##### Naming Policies

Naming policies determine what action RECAP should take when no explicit name is provided for an entity or property. They can be configured by adding the `EntityNaming` policy to the policy option builder. The following predefined naming policies are available:

| Policy | Description |
| --- | --- |
| `AllowImplicit` | Allows implicit naming of database column, tables, or views determined automatically from the corresponding CLR name via EF Core conventions. |
| `PreferExplicit` | Allows implicit naming of database components, determined automatically from the corresponding property or CLR type name via EF Core conventions but generates warnings and advises against implicit naming in an attempt to pressure you into writing clean code. *This is the default naming policy used by RECAP.* |
| `RequireExplicit` | Requires explicit naming of all database components and enforces this policy by throwing an exception if implicit naming attempts are detected. |

##### Mapping Policies

Mapping policies determine what action RECAP should take when a property is neither ignored nor explicitly mapped. The following predefined mapping policies are available:

| Policy | Description |
| --- | --- |
| `AllowImplicit` | Allows implicit, convention-based mapping of properties. This is the default behavior of EF Core. If no mapping is provided, EF Core will attempt to automatically map the property to a database column with the same name. |
| `PreferExplicit` | Prefers explicit mapping of properties, but allows implicit, automatic convention-based mapping by EF Core and generates a warning if such a mapping is encountered. |
| `RequireExplicit` | Requires explicit mapping of properties and enforces this policy by throwing an exception if implicit mapping is attempted. |
| `IgnoreImplicit` | Automatically ignores properties that are not explicitly mapped. This has the same effect as calling the `Ignore()` method on the `PropertyBuilder` instance. *This is the default mapping policy used by RECAP.* |

##### Inheritance Policies

If you are using soft delete or change tracking techniques that require all entities to inherit from a common base class, you can enforce such inheritance requirements using the predefined `EntityInheritanceValidation` policy, which automatically enforces the configured inheritance requirements on all entities loaded via Reflective Entity Discovery.

```csharp
class MyDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.LoadReflectiveModels(options => options
            .ConfigurePolicies(policies => policies
                .AddPolicy<EntityInheritanceValidation>(entity => entity
                    .MustExtend<ISoftDeletable>()
                    .UnlessExtends<IDatabaseView>()
                    .Unless<TransientFoo>())
                .AddPolicy<EntityInheritanceValidation>(entity => entity
                    .ShouldExtend<IChangeTrackable>()
                    .UnlessExtends<IDatabaseView>()
                    .Unless<TransientFoo>())
                    .Unless<TransientBar>()));
    }
}
```

In the example above, the `EntityInheritanceValidation` policy enforces that all entities loaded via Reflective Entity Discovery must inherit from the `ISoftDeletable` interface, unless they inherit from the `IDatabaseView` interface or are of the `TransientFoo` type, which are exempt from the inheritance requirement. If an entity violates the inheritance requirements, an appropriate action is taken, such as throwing an exception or generating a warning. The severity of the action depends on whether the requirement is mandatory (`MustExtend`) or optional (`ShouldExtend`). Multiple independent inheritance requirements can be enforced by adding multiple `EntityInheritanceValidation` policies.

##### Custom Policies

RECAP's policy system is designed to be extensible, allowing you to create custom policies that apply to your specific needs and requirements. Custom policies are implemented by specifying a builder class that implements `IEntityPolicyBuilder<TSelf>` and a policy class that implements `IEntityPolicy`. The following examples aim to demonstrate the versatility of custom policies and may be used as a starting point for creating your own custom policies today.

> :x: **Caution**
> Policies are designed to be stateless and immutable. If you absolutely need to store state, beware that policy objects are treated as singletons by RECAP, so the same instance will be used for all mappings.

###### Example: Code-First Naming with Snake Case

Assume you are using a code-first approach to define your entities and properties, and want EF Core to automatically generate database tables and columns with snake case names, but you don't want to use data annotations or fluent API to specify every table and column name explicitly. You can create a custom naming policy that converts the default PascalCase names to snake_case names by convention.

<details>
<summary>

*Show/hide code-snippet*

</summary>

```csharp
// a minimalistic policy builder class with no additional configuration options
public class UseSnakeCaseByConvention : IEntityPolicyBuilder<UseSnakeCaseByConvention>
{
    // the factory method to create the policy builder
    static UseSnakeCaseByConvention IEntityPolicyBuilder<UseSnakeCaseByConvention>.Create() => new();

    // only one instance of the policy is needed
    static bool IEntityPolicyBuilder.AllowMultiple => false;

    // build the policy
    IEntityPolicy IEntityPolicyBuilder.Build() => new Policy();

    // the internal policy class that actually enforces the naming convention
    private class Policy : IEntityPolicy
    {
        public void Audit(IMutableEntityType entityType)
        {
            // unless explicitly specified, convert the entity name to snake_case
            if (!entityType.HasAnnotation(annotation => annotation 
                is RelationalAnnotationNames.TableName 
                or RelationalAnnotationNames.ViewName))
            {
                entityType.SetTableName(ToSnakeCase(entityType.Name));
            }
            foreach (var property in entityType.GetProperties())
            {
                // same for properties
                if (!property.HasAnnotation(annotation => annotation is RelationalAnnotationNames.ColumnName))
                {
                    property.SetColumnName(ToSnakeCase(property.Name));
                }
            }
        }

        private static string ToSnakeCase(string name)
        {
            StringBuilder bobTheBuilder = new();
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (i > 0 && char.IsUpper(c))
                {
                    builder.Append('_');
                }
                builder.Append(char.ToLowerInvariant(c));
            }
            return builder.ToString();
        }
    }
}

// usage in the DbContext
class MyDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.LoadReflectiveModels(options => options
            .ConfigurePolicies(policies => policies
                .AddPolicy<UseSnakeCaseByConvention>()));
    }
}
```

</details>

###### Example: Ensure All UUID Properties are Database-Generated

Assume you are using UUIDs as substitute primary keys for your entities and want to ensure that all UUID properties are automatically generated by the database (rather being provided by the client). Perhaps you are using an `IGeneratedUuidOwner` interface to mark entities that own a UUID property that should be automatically generated. This interface may then be used in the `DbContext` to ignore changes to the UUID properties when saving changes to the database.

You can create a custom policy that enforces that all UUID properties are automatically generated by the database. The following example demonstrates how to create such a policy:

<details>
<summary>

*Show/hide code-snippet*

</summary>

```csharp
// a policy builder class with configuration options
internal class AssertUuidsAreDatabaseGenerated : IEntityPolicyBuilder<AssertUuidsAreDatabaseGenerated>
{
    private AssertionAction _action = AssertionAction.Warn;
    private Func<IMutableProperty, bool>? _propertyFilter;

    // having multiple instances of the policy is nonsensical and not allowed
    static bool IEntityPolicyBuilder.AllowMultiple => false;

    static AssertUuidsAreDatabaseGenerated IEntityPolicyBuilder<AssertUuidsAreDatabaseGenerated>.Create() => new();

    IEntityPolicy? IEntityPolicyBuilder.Build() => new Policy(_action, _propertyFilter ?? (prop => true));

    // configuration methods
    // determines the action to take when a violation is detected
    public AssertUuidsAreDatabaseGenerated OnViolation(AssertionAction action)
    {
        _action = action;
        return this;
    }

    // filters the properties to which the policy should be applied
    public AssertUuidsAreDatabaseGenerated ForProperties(Func<IMutableProperty, bool> propertyFilter)
    {
        if (_propertyFilter is null)
        {
            _propertyFilter = propertyFilter;
        }
        else
        {
            Func<IMutableProperty, bool> oldFilter = _propertyFilter;
            _propertyFilter = prop => oldFilter.Invoke(prop) && propertyFilter.Invoke(prop);
        }
        return this;
    }

    private class Policy(AssertionAction action, Func<IMutableProperty, bool> propertyFilter) : IEntityPolicy
    {
        public void Audit(IMutableEntityType entityType)
        {
            if (!entityType.ClrType.ImplementsInterface<IGeneratedUuidOwner>() 
                && entityType.GetProperties().Any(prop => prop.ClrType == typeof(Uuid) 
                && propertyFilter.Invoke(prop)))
            {
                string message = $"Entity type {entityType.ClrType.Name} has a UUID property but does not implement {nameof(IGeneratedUuidOwner)}. This may cause issues with UUID generation.";
                Log.WriteWarning(message, LogWriter.Blocking);
                if (action == AssertionAction.Throw)
                {
                    throw new PolicyViolationException(message);
                }
            }
        }
    }
}

// for configuration
public enum AssertionAction
{
    Warn,
    Throw,
}

// the interface to mark entities that own a UUID property
public interface IGeneratedUuidOwner
{
    Uuid Uuid { get; }
}

// usage in the DbContext
class MyDbContext
{
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        // Prevent setting Uuid manually (database will generate it)
        foreach (EntityEntry<IGeneratedUuidOwner> entry in ChangeTracker.Entries<IGeneratedUuidOwner>())
        {
            // automatically rollback changes to database-generated UUIDs
            PropertyEntry<IGeneratedUuidOwner, Uuid> uuid;
            if (entry.State is EntityState.Added or EntityState.Modified 
                && (uuid = entry.Property(e => e.Uuid)).IsModified)
            {
                uuid.IsModified = false;
#if DEBUG
                if (uuid.CurrentValue != default && uuid.CurrentValue != uuid.OriginalValue)
                {
                    throw new InvalidOperationException($"Uuid must not be set manually. Uuid values of owner {nameof(IGeneratedUuidOwner)} entities are generated automatically.");
                }
#endif
            }
        }
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.LoadReflectiveModels(options => options
            .ConfigurePolicies(policies => policies
                .AddPolicy<AssertUuidsAreDatabaseGenerated>(assertUuid => assertUuid
                    // there may be some UUID properties that have legitimate reasons to be set manually
                    // e.g., Camunda UUID references. These should be excluded from the policy.
                    // usually, these properties should be named more expressively than just "Uuid",
                    // so we can use a simple name filter to exclude them
                    .ForProperties(prop => prop.Name == nameof(IGeneratedUuidOwner.Uuid))
                    .OnViolation(AssertionAction.Throw))));
    }
}
```

</details>

###### Example: Automatically Ignoring Navigation Properties during JSON Serialization

Assume you are using JSON serialization to serialize your entities into API responses, and you want to automatically ignore navigation properties to prevent circular references and reduce the size of the JSON response. You can create a custom policy in conjunction with the a custom JSON converter to automatically ignore navigation properties during JSON serialization.

<details>
<summary>

*Show/hide code-snippet*

</summary>

```csharp
// The JSON converter that automatically ignores navigation properties
internal class IgnoreComplexPropertiesJsonConverter<T>(ImmutableArray<PropertyInfo> allowedProperties) : JsonConverter<T> where T : class
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => 
        JsonSerializer.Deserialize<T>(ref reader, options);

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        for (int i = 0; i < allowedProperties.Length; i++)
        {
            PropertyInfo property = allowedProperties[i];
            object? propValue = property.GetValue(value);
            writer.WritePropertyName(property.Name);
            JsonSerializer.Serialize(writer, propValue, options);
        }
        writer.WriteEndObject();
    }
}

// The policy that automatically applies the JSON converter to all entities
public class JsonIgnoreComplexProperties : IEntityPolicyBuilder<JsonIgnoreComplexProperties>
{
    static bool IEntityPolicyBuilder.AllowMultiple => false;

    static JsonIgnoreComplexProperties IEntityPolicyBuilder<JsonIgnoreComplexProperties>.Create() => new();

    IEntityPolicy? IEntityPolicyBuilder.Build() => new Policy();

    private class Policy : IEntityPolicy
    {
        public void Audit(IMutableEntityType entityType)
        {
            ImmutableArray<PropertyInfo> allowedProperties = entityType.GetProperties()
                .Where(prop => !prop.IsShadowProperty())
                .Select(prop => prop.PropertyInfo)
                .Where(prop => prop is not null)
                .ToImmutableArray()!;
            Type converterType = typeof(IgnoreComplexPropertiesJsonConverter<>).MakeGenericType(entityType.ClrType);
            JsonConverter converter = (JsonConverter)(Activator.CreateInstance(converterType, allowedProperties)
                ?? throw new InvalidOperationException($"Failed to create an instance of {converterType}."));
            Log.WriteDiagnostic($"Added {nameof(IgnoreComplexPropertiesJsonConverter<object>)} to {entityType.ClrType.Name}. Included properties: {string.Join(", ", allowedProperties.Select(prop => prop.Name))}");
            SerializationStore.SerializerOptions.Converters.Add(converter);
        }
    }
}

// Usage in the DbContext
class MyDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.LoadReflectiveModels(options => options
            .ConfigurePolicies(policies => policies
                .AddPolicy<JsonIgnoreComplexProperties>()));
    }
}
```

</details>

### Stored Procedure Mapping

RECAP allows you to map stored procedures and database functions to CLR command objects (so-called *Procedure Command Objects (PCOs)*). PCOs are similar to entities in EF Core in that they are mapped to database objects and require configuration via fluent API. However, unlike entities, PCOs are generally stateless and specific to the database provider.

Every mapped procedure or function consist of three components in RECAP:

- A ***Procedure Command Object (PCO)*** class that represents the procedure or function itself. The PCO class is used by client code to invoke the procedure or function.
- An ***Input/Output Container (I/O Container)*** class that represents the input and output parameters, and scalar return values of the procedure or function. The I/O Container class is used by the PCO class to pass parameters to the procedure or function and to retrieve the results.
- An optional ***Result*** class that represents the result set of a procedure invocation. The Result class is similar to an entity class and is used to map a set of columns returned by a procedure to a CLR type. A procedure may return a single result or a set of multiple results.

> :information_source: **Note**
RECAP internally uses ADO.NET to invoke stored procedures and functions. Therefore, binding libraries for the target database provider must be installed in the project. For more information, see [Supported Database Providers](#requirements).

#### Getting Started with PCO Mapping

The following examples show how to map stored procedures and functions in RECAP. The examples provided here are targeting MySQL, but the same principles apply to other database providers.

> :bulb: **Tip**
> For full documentation on the options available for your target database provider, see the documentation for the [binding library of your database provider](#requirements).

##### Mapping a Database Function

The following example shows how to map a database function that takes two integer parameters and returns the sum of the two parameters.

Assuming the following database function is defined in the database:

<details>
<summary>

*Show/hide <code>perform_addition</code> database function definition*

</summary>

```sql
CREATE FUNCTION perform_addition (IN a INT, IN b INT) RETURNS INT
BEGIN
    RETURN a + b;
END
```

</details>

The first step is to define a PCO class that represents the function. The PCO class must inherit from the provider-specific `StoredProcedure<TContainer>` implementation, where `TContainer` is the type of the I/O Container class that represents the input and output parameters of the function. In this example we are using MySQL, so we inherit from the `MySqlStoredProcedure<TContainer>` class. 

Depending on whether you use Reflective or Manual Procedure Discovery, the PCO class must additionally implement either `IProcedureConfiguration<TProcedure, TContainer>` or `IReflectiveProcedureConfiguration<TProcedure, TContainer>`, where `TProcedure` is the type of the PCO class itself and `TContainer` is the type of the I/O Container class. In this example we are using Reflective Procedure Discovery, so we implement `IReflectiveProcedureConfiguration<TProcedure, TContainer>`. Either way, a `Configure()` method must be implemented to configure the PCO class. The following example shows how to configure the PCO class for the `perform_addition` function:

<details>
<summary>

*Show/hide <code>Addition</code> PCO class definition*

</summary>

```csharp
// define a PCO class that represents the function
public class Addition : MySqlStoredProcedure<AdditionContainer>,
    IReflectiveProcedureConfiguration<Addition, AdditionContainer>
{
    // depending on your use case, you can define any number of Invoke() methods
    // with different signatures or method names. the method name does not matter.
    public int Invoke(int a, int b)
    {
        // create an I/O Container instance
        AdditionContainer io = new(a, b, default);
        // invoke the function by calling the base class implementation
        Execute(io);
        // retrieve the result from the I/O Container and return it
        return io.Result;
    }
    // map parameters and return values of the function to properties of the I/O Container
    public static void Configure(MySqlProcedureBuilder<Addition, AdditionContainer> self)
    {
        // map the function name
        _ = self.ToDatabaseFunction("perform_addition") 
            .ReturnsScalar(io => io.Result) // configure the result
            .HasDbType(MySqlDbType.Int32);  // configure the DB return type
        // configure the first parameter
        _ = self.Parameter(io => io.A)
            .HasName("a")                   // map the parameter name
            .HasDbType(MySqlDbType.Int32);  // configure the DB type
        // configure the second parameter
        _ = self.Parameter(io => io.B)
            .HasName("b")                   // map the parameter name
            .HasDbType(MySqlDbType.Int32);  // configure the DB type
    }
}
// define an I/O Container class that represents the input and output parameters
// of the function.
public record AdditionContainer(int A, int B, int Result);
```

</details>

As shown in the example above, the `Configure()` method is used to map the function to the PCO class. The `ToDatabaseFunction()` method is used to map the function name. The `ReturnsScalar()` method is used to map the scalar return value of the function. The `Parameter()` method is used to map the input parameters of the function, where `HasName()` is used to map the parameter name, and `HasDbType()` specifies the database type of the parameter.

> :bulb: **Tip**
> RECAP provides a `ReturnsScalar(...)` extension method to map scalar return values of stored procedures. This extension method is functionally equivalent to the `Parameter(...).HasDirection(ParameterDirection.ReturnValue)` method chain.

The example above uses a record type for the I/O Container class to take advantage of the built-in deconstruction and primary constructor features of records. However, you can use any reference type as an I/O Container class. The only restriction is that only properties can be mapped to parameters and return values. Fields are not supported.

> :bulb: **Tip**
> Even output properties of the I/O Container class don't have to be writable. RECAP uses dynamic IL emission to access I/O Container properties, so records with init-only properties are fully supported.

RECAP does not require any specific signatures for invoking the PCO class. You can freely define any number of instance methods with different signatures or names to invoke the function. By convention, the name *"Invoke"* should be used for the method that is used by client code to invoke the function (duh). However, this is not a requirement.

##### Mapping a Stored Procedure

The following example shows how to map a stored procedure with input and output parameters that returns a result set.

Assuming the following stored procedure is defined in the database:

<details>
<summary>

*Show/hide <code>get_persons_by_name</code> stored procedure definition*

</summary>

```sql
CREATE PROCEDURE get_persons_by_name (IN name_in VARCHAR(255), OUT invalid_count INT)
BEGIN
    SET invalid_count = (SELECT COUNT(*) FROM person WHERE `name` IS NULL);
    SELECT `id`, `name`, `uuid` FROM `person` WHERE `name` = name_in;
END
```

</details>

The procedure takes a single input parameter `name_in` and an output parameter `invalid_count`, which is used to return the number of rows in the `person` table where the `name` column is `NULL`. The procedure returns a result set with three columns: `id`, `name`, and `uuid`.

Mapping a stored procedure in RECAP is very similar to mapping a function. The only differences are that the `ToDatabaseFunction()` method is replaced with the `ToDatabaseProcedure()` method, and that parameter and result set mappings are a bit more complex. The following example shows how the `get_persons_by_name` procedure can be mapped:

<details>
<summary>

*Show/hide <code>GetPersonsByName</code> PCO class definition*

</summary>

```csharp
// define a PCO class that represents the procedure
public class GetPersonsByName : MySqlStoredProcedure<GetPersonsByNameContainer, GetPersonsByNameResult>,
    IReflectiveProcedureConfiguration<GetPersonsByName, GetPersonsByNameContainer>
{
    public IReadOnlyList<GetPersonsByNameResult> Invoke(string name, out int invalidCount)
    {
        // create an I/O Container instance
        GetPersonsByNameContainer io = new(name, default);
        // invoke the procedure by calling the base class implementation
        IResultContainer<GetPersonsByNameResult> result = Execute(io);
        // set the output parameter
        invalidCount = io.InvalidCount;
        // retrieve the result as a collection and return it
        return result.AsCollection();
    }

    public static void Configure(MySqlProcedureBuilder<GetPersonsByName, GetPersonsByNameContainer> self)
    {
        _ = self.ToDatabaseProcedure("get_persons_by_name");
        // unless specified otherwise, RECAP assumes that all parameters are input parameters.
        _ = self.Parameter(io => io.Name)
            .HasName("name_in")
            .HasDbType(MySqlDbType.String)
            .HasSize(255);
        // configure the output parameter
        _ = self.Parameter(io => io.InvalidCount)
            .HasName("invalid_count")
            .HasDirection(ParameterDirection.Output);
        // configure the result set and tell RECAP to read *all* returned rows.
        MySqlResultBuilder<GetPersonsByNameResult> result = self
            .Returns<GetPersonsByNameResult>()
            .AsCollection();
        // configure the columns of the result set
        // RECAP will attempt to automatically determine the DB type based on the
        // CLR type of the property. If this is not possible, you can use the
        // HasDbType() method to specify the DB type manually.
        _ = result.Column(io => io.Id)
            .HasName("id");
        _ = result.Column(io => io.Name)
            .HasName("name");
        // in this example, the UUID column is stored as a BINARY(16) column in the database.
        // => tell RECAP to read the column as a byte array and provide a conversion function
        //    that converts the byte array to a Guid.
        _ = result.Column(io => io.Uuid)
            .HasName("uuid")
            .GetAsBytes()
            .RequiresConversion(bytes => new Guid(bytes));
    }
}
// define an I/O Container class that represents the input and output parameters
// of the function.
public record GetPersonsByNameContainer(string Name, int InvalidCount);

// define a result class that represents a single row of the result set returned by this procedure
public record GetPersonsByNameResult(int Id, string Name, Guid Uuid);
```

</details>

As shown above, PCOs expecting a result set must inherit from the `StoredProcedure<TContainer, TResult>` base class, where `TContainer` is the I/O Container class, and `TResult` is the result class. 

The `Configure()` method is used to map the procedure to the PCO class. The `ToDatabaseProcedure()` method tells RECAP to invoke `get_persons_by_name` as a procedure (instead of a function). The `Parameter()` method is used to configure the input and output parameters of the procedure. 

Then the `Returns<T>()` method is used to configure the result set. The `AsCollection()` method tells RECAP to eagerly read all returned rows into a collection. Alternatively `AsSingle()` can be used if you only expect a single row to be returned, or if are only interested in the first row.

The `Column()` method of the `ResultBuilder` is used to configure the columns of the result set. The `id` and `name` columns are mapped using RECAP's support for inferred database types. The `uuid` column is mapped using a custom conversion function, because the `uuid` column is stored as a `BINARY(16)` column in the database, which represents a complex GUID type in .NET. The `GetAsBytes()` method tells RECAP to read the column as a byte array, while the `RequiresConversion()` method provides a conversion function that converts the byte array to a `Guid` which matches the type of the `Uuid` property. Without this conversion function, RECAP would throw an exception because it cannot convert the byte array to a `Guid` automatically.

> :x: **Caution**
> When instantiating Result classes, RECAP requires that the class has a public constructor with parameters matching the columns of the result set. Parameter names are compared case-insensitively, CLR types must match exactly with the mapped Property types. If no matching constructor is found, RECAP will throw an exception. It is recommended to use primary constructor syntax for result classes to avoid this problem.

#### PCO Discovery

Similar to entities, RECAP supports automatic reflective discovery of PCO classes. The following example shows how to discover and load all PCO classes in the current assembly:

```csharp
class MyDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        builder
            // load entities as usual
            .LoadReflectiveModels(EntityNamingPolicy.RequireExplicit, PropertyMappingPolicy.IgnoreImplicit)
            // discover and load PCO definitions
            .LoadReflectiveProcedures();
    }
}
```

As always, you can also opt to load PCO classes manually (without reflection):

```csharp
class MyDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        builder
            // load entities as usual
            .LoadModel<Person>()
            .LoadModel<Child>()
            // load PCO classes with corresponding I/O Containers manually 
            .LoadProcedure<Addition, AdditionContainer>()
            .LoadProcedure<GetPersonsByName, GetPersonsByNameContainer>();
    }
}
```

### Executing PCOs

Once a PCO class has been defined and mapped, it can be used in a similar way as an entity. Instead of calling `Set<T>()` on the `DbContext`, you can call `Procedure<T>()` to get an invocable instance of the PCO.

The following example shows how to invoke the `Addition` PCO:

```csharp
using (MyDbContext dbContext = new())
{
    int result = dbContext.Procedure<Addition>().Invoke(1, 2);
    Console.WriteLine(result); // prints 3
}
```

PCOs are transaction-aware, which means that they will automatically enlist in the current transaction if one exists. If no transaction exists, RECAP will not create a new transaction. This means that PCOs can be used in the same way as entities, and you can perform database operations using EF Core and PCOs in the same transaction:

```csharp
using MyDbContext dbContext = new();
using IDbContextTransaction transaction = dbContext.Database.BeginTransaction();

dbContext.Add(new Person { Name = "John" });
dbContext.SaveChanges();

IReadOnlyList<GetPersonsByNameResult> results = dbContext.Procedure<GetPersonsByName>()
    .Invoke("John", out int _);
Console.WriteLine(results.Count > 0); // prints True

transaction.Commit(); // or transaction.Rollback();
```

You can also re-use the same PCO instance to invoke the procedure multiple times:

```csharp
using MyDbContext dbContext = new();
IResultContainer<int> addition = dbContext.Procedure<Addition>();
int result1 = addition.Invoke(1, 2);
int result2 = addition.Invoke(3, 4);
Console.WriteLine(result1); // prints 3
Console.WriteLine(result2); // prints 7
```

> :warning: **Warning**
> When re-using a PCO instance, you are re-using the internal Execution Context (state) of the PCO. This means that PCOs are not thread-safe. If you need to invoke a PCO from multiple threads, you must create a separate PCO instance using a separate `DbContext` instance for each thread. Similarly, do not re-use PCO instances across multiple transactions.