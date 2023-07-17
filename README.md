# WKG Entity Framework Core

![](https://git.wkg.lan/WKG/components/wkg-entity-framework-core/badges/main/pipeline.svg)

---

## Abstract

*Object-relational mapping (ORM) is a widely used technique in software development to manage the interaction between relational databases and object-oriented programming languages. It allows developers to work with the database using familiar object-oriented concepts rather than having to write raw SQL queries. However, given the risk of increased maintenance costs and decreased developer productivity posed by inefficient ORM configurations for large-scale software systems with complex and interconnected business logic, it is vital to ensure that these configurations are as clear and concise as possible.*<br>
*The Reflective Entity Configuration And Procedure mapping framework (RECAP) has been designed and developed as a solution to simplify, standardize, and automate the configuration of relational database access layers in .NET. By building upon the existing functionality of Microsoft's Entity Framework Core, RECAP streamlines the configuration process for entity mappings and stored database procedures in .NET, resulting in improved maintainability and reduced code redundancy. This is achieved through the use of a uniform configuration syntax and the ability to map procedures to entity-like command objects.*

---

This repository contains the database-independant core components of the RECAP framework as well as other re-usable components not directly related to RECAP. For platform-specific implementations of RECAP, see the following repositories:

- [WKG Entity Framework Core Oracle](https://git.wkg.lan/WKG/components/wkg-entity-framework-core-oracle)
- [WKG Entity Framework Core MySQL](https://git.wkg.lan/WKG/components/wkg-entity-framework-core-mysql)

## Installation

The *WKG Entity Framework Core* library is available as a NuGet package from our internal nuget feed. To install it, add the following package source to your NuGet configuration:

```xml
<PropertyGroup>
    <RestoreAdditionalProjectSources>
        https://baget.wkg.lan/v3/index.json
    </RestoreAdditionalProjectSources>
</PropertyGroup>
```

Then, install the package by adding the following package reference to your project file:

```xml
<ItemGroup>
    <PackageReference Include="Wkg.EntityFrameworkCore" Version="X.X.X" />
</ItemGroup>
```

> :warning: **Warning**
> Replace `X.X.X` with the latest stable version available on the [nuget feed](https://baget.wkg.lan/packages/wkg.entityframeworkcore/latest), where **the major version must match the major version of your targeted .NET runtime**.

## Conceptual Design and Usage

For the full theoretical background and design principles of RECAP, please refer to the [RECAP paper](/docs/RECAP-concept.pdf).

For technical documentation and usage examples, please refer to the [documentation](/docs/documentation.md).