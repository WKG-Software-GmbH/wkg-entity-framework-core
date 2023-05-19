# Documentation

## Table of Contents

- [Documentation](#documentation)
  - [Table of Contents](#table-of-contents)
  - [Getting Started](#getting-started)
    - [Installation](#installation)
  - [Usage](#usage)

## Getting Started

### Installation

The *WKG Entity Framework Core* library is available as a NuGet package from out internal nuget feed. To install it, add the following package source to your NuGet configuration:

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

> **Note**
> Replace `X.X.X` with the latest version available on the [nuget feed](https://baget.wkg.lan/packages/wkg.entityframeworkcore).

## Usage

TODO