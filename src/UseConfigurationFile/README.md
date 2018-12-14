# Template Quick Start

|Ver|Date|Author|Remark
|:-|:-:|:-|:-
|1.0|2018-12-14|kuro|created

## *References:*

---

### Table of Contents

* [Logs](#Logs)
* [Chapter 2](#Chapter-2)

---

### Logs

---

1. add packages
    * ```dotnet add package Microsoft.Extensions.Configuration```
    * ```dotnet add package Microsoft.Extensions.Configuration.Json```
    * ```dotnet restore```
1. add a config file named ```sample1.json```
1. edit Sample1.csproj

    add following node
    ```xml
    <ItemGroup>
        <Content Include="sample1.json">
        <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </ItemGroup>
    ```

### Chapter 2

---