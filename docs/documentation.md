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
      - [Defining Mapping and Naming Policies](#defining-mapping-and-naming-policies)
        - [Naming Policies](#naming-policies)
        - [Mapping Policies](#mapping-policies)
        - [Applying Policies](#applying-policies)
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

> :warning:
> In order to reduce the risk of missing entities, it is recommended to use [Reflective Entity Discovery](#reflective-entity-discovery), rather than manual registration.

#### Configuring Inheritance Hierarchies

RECAP supports all inheritance strategies supported by EF Core, including Table Per Hierarchy (TPH), Table Per Type (TPT), and Table Per Concrete Type (TPC). The following examples aim to demonstrate how to configure each of these inheritance strategies.

##### Table Per Hierarchy (TPH)

Table Per Hierarchy (TPH) is the default inheritance strategy used by EF Core. In this strategy, all entities in the inheritance hierarchy are mapped to a single table, and a discriminator column is used to differentiate between the different entity types. 

In RECAP, TPH inheritance is achieved by configuring the discriminator column in the base entity mapping, and by mapping any additional properties in the derived entity mappings. The following example shows how to configure TPH inheritance for the `Person`, `Child`, and `Adult` entities:

<details>
<summary style="font-style: italic">Show/hide <code>Person</code> TPH configuration</summary>

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
<summary style="font-style: italic">Show/hide <code>Adult</code> TPH configuration</summary>

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
<summary style="font-style: italic">Show/hide <code>Child</code> TPH configuration</summary>

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
<summary style="font-style: italic">Show/hide <code>Person</code> TPT configuration</summary>

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
<summary style="font-style: italic">Show/hide <code>Adult</code> TPT configuration</summary>

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
<summary style="font-style: italic">Show/hide <code>Child</code> TPT configuration</summary>

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
<summary style="font-style: italic">Show/hide <code>Person</code> TPC configuration</summary>

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
<summary style="font-style: italic">Show/hide <code>Adult</code> TPC configuration</summary>

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
<summary style="font-style: italic">Show/hide <code>Child</code> TPC configuration</summary>

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
<summary style="font-style: italic">Show/hide <code>Person</code> entity configuration</summary>

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
<summary style="font-style: italic">Show/hide <code>Group</code> entity configuration</summary>

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
<summary style="font-style: italic">Show/hide <code>PersonToGroup</code> connection entity configuration</summary>

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

#### Defining Mapping and Naming Policies

RECAP allows you to use predefined or custom mapping and naming policies to enforce different policies for your Entity Framework mappings. Following the principles of *"explicit is better than implicit"*, you can, for example, prohibit EF Core from applying convention-based mappings if no explicit column or table name has been specified. This way, you can ensure that all mappings are explicitly defined and that renaming a property or class will not break your application.

##### Naming Policies

Naming policies determine what action RECAP should take when no explicit column or table name is provided for an entity or property. The following predefined naming policies are available:

| Policy | Description |
| --- | --- |
| `AllowImplicit` | Allows implicit naming of database column, tables, or views determined automatically from the corresponding CLR name via EF Core conventions. |
| `PreferExplicit` | Allows implicit naming of database columns determined automatically from the corresponding property name via EF Core conventions but generates warnings and advises against implicit naming in an apttempt to pressure you into writing clean code. *This is the default naming policy used by RECAP.* |
| `RequireExplicit` | Requires explicit naming of database columns and enforces this policy by throwing an exception if implicit naming is attempted. |

Predifined naming policies are provided as static properties of the `NamingPolicy` class.

You can also create your own custom naming policies by implementing the `INamingPolicy` interface. The following example shows how to create a naming policy that allows convention-based naming of database columns and tables but ensures that the names are in snake case:

```csharp
readonly struct UseSnakeCaseByConvention : INamingPolicy
{
    public void Audit(IMutableEntityType entityType)
    {
        if (!entityType.HasAnnotation(annotation => annotation 
            is RelationalAnnotationNames.TableName 
            or RelationalAnnotationNames.ViewName))
        {
            entityType.SetTableName(ToSnakeCase(entityType.Name));
        }
        foreach (var property in entityType.GetProperties())
        {
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
```

> :x: **Caution**
> Policies are designed to be stateless and immutable. If you absolutely need to store state, beware that policy objects are treated as singletons by RECAP, so the same instance will be used for all mappings.

##### Mapping Policies

Mapping policies determine what action RECAP should take when a property is neither ignored nor explicitly mapped. The following predefined mapping policies are available:

| Policy | Description |
| --- | --- |
| `AllowImplicit` | Allows implicit, convention-based mapping of properties. This is the default behavior of EF Core. If no mapping is provided, EF Core will attempt to automatically map the property to a database column with the same name. |
| `PreferExplicit` | Prefers explicit mapping of properties, but allows implicit, automatic convention-based mapping by EF Core and generates a warning if such a mapping is encountered. |
| `RequireExplicit` | Requires explicit mapping of properties and enforces this policy by throwing an exception if implicit mapping is attempted. |
| `IgnoreImplicit` | Automatically ignores properties that are not explicitly mapped. This has the same effect as calling the `Ignore()` method on the `PropertyBuilder` instance. *This is the default mapping policy used by RECAP.* |

Predifined mapping policies are provided as static properties of the `PropertyMappingPolicy` class.

Similar to naming policies, you can also create your own custom mapping policies by implementing the `IMappingPolicy` interface.

##### Applying Policies

You can apply naming and mapping policies by passing corresponding policy objects to the `LoadReflectiveModels()` method. The following example shows how to apply the `AllowImplicit` naming policy and the `IgnoreImplicit` mapping policy:

```csharp
class MyDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.LoadReflectiveModels
        (
            NamingPolicy.AllowImplicit, 
            PropertyMappingPolicy.IgnoreImplicit
        );
    }
}
```

If you are using manual entity registration, you can use an `IDiscoveryContext` instance to apply policies to all loaded entities. The following example shows how to apply the `PreferExplicit` naming policy and the `IgnoreImplicit` mapping policy manually:

```csharp
class MyDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // create a discovery context with the desired policies
        IDiscoveryContext discoveryContext = modelBuilder.CreateDiscoveryContext
        (
            NamingPolicy.PreferExplicit, 
            PropertyMappingPolicy.IgnoreImplicit
        );
        // load entities and connections using the discovery context
        _ = modelBuilder
            .LoadModel<Person>(discoveryContext)
            .LoadModel<Group>(discoveryContext)
            .LoadConnection<PersonToGroup, Person, Group>(discoveryContext);
        // enforce policies on all loaded entities
        discoveryContext.AuditPolicies();
    }
}
```

### Stored Procedure Mapping

TODO