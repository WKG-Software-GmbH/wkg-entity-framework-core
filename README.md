# WKG Entity Framework Core

[![NuGet version (Wkg.EntityFrameworkCore)](https://img.shields.io/nuget/v/Wkg.EntityFrameworkCore.svg?style=flat-square)](https://www.nuget.org/packages/Wkg.EntityFrameworkCore/)

---

Wkg.EntityFrameworkCore is a company-internal library providing the Reflective Entity Configuration And Procedure mapping extension (RECAP) to simplify the configuration of Entity Framework Core models, introduce ORM-compatible stored procedure and function mapping, add support for UUID data types, policy validation for discovered entities, and more. The library helps to reduce the amount of boilerplate code required to configure Entity Framework Core models and stored procedures, bringing standardization, validation, and easier maintenance to our projects at WKG Software GmbH.

As part of our commitment to open-source software, we are making this library [available to the public](https://github.com/WKG-Software-GmbH/wkg-entity-framework-core) under the GNU General Public License v3.0. We hope that it will be useful to other developers and that the community will contribute to its further development.

## Abstract

*Object-relational mapping (ORM) is a widely used technique in software development to manage the interaction between relational databases and object-oriented programming languages. It allows developers to work with the database using familiar object-oriented concepts rather than having to write raw SQL queries. However, given the risk of increased maintenance costs and decreased developer productivity posed by inefficient ORM configurations for large-scale software systems with complex and interconnected business logic, it is vital to ensure that these configurations are as clear and concise as possible.*<br>
*The Reflective Entity Configuration And Procedure mapping framework (RECAP) has been designed and developed as a solution to simplify, standardize, and automate the configuration of relational database access layers in .NET. By building upon the existing functionality of Microsoft's Entity Framework Core, RECAP streamlines the configuration process for entity mappings and stored database procedures in .NET, resulting in improved maintainability and reduced code redundancy. This is achieved through the use of a uniform configuration syntax and the ability to map procedures to entity-like command objects.*

---

This repository contains the database-independant core components of the RECAP framework as well as other re-usable components not directly related to RECAP. For database-specific implementations of RECAP, required for stored procedure mapping, see the following repositories:

- [WKG Entity Framework Core Oracle](https://github.com/WKG-Software-GmbH//wkg-entity-framework-core-oracle)
- [WKG Entity Framework Core MySQL](https://github.com/WKG-Software-GmbH//wkg-entity-framework-core-mysql)

## Installation

Install the `Wkg.EntityFrameworkCore` package by adding the following package reference to your project file:

```xml
<ItemGroup>
    <PackageReference Include="Wkg.EntityFrameworkCore" Version="X.X.X" />
</ItemGroup>
```

> :warning: **Warning**
> Replace `X.X.X` with the latest stable version available on the [nuget feed](https://www.nuget.org/packages/Wkg.EntityFrameworkCore), where **the major version must match the major version of your targeted .NET runtime**.

## Conceptual Design and Usage

For the full theoretical background and design principles of RECAP, please refer to the [RECAP paper](https://github.com/WKG-Software-GmbH/wkg-entity-framework-core/tree/main/docs/RECAP-concept.pdf).

For technical documentation and usage examples, please refer to the [documentation](https://github.com/WKG-Software-GmbH/wkg-entity-framework-core/tree/main/docs/documentation.md).