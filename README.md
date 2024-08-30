# Changes to Run

* `Directory.Build.props` `RUNTIME_ID` to `win-x64` - for windows run
* Add from NuGet `YamlDotNet@12.0.0.0`
* Add from NuGet `Serilog@2.0.0.0`
* Add from Nuget `Microsoft.Orleans.Clustering.AdoNet@2.0.0`

Update on RUNTIME_ID:

```
  <PropertyGroup>
    <RUNTIME_ID Condition="'$(RUNTIME_ID)' == ''">win-x64</RUNTIME_ID>
  </PropertyGroup>
```

Add to the csproj:
```
<PackageReference Include="YamlDotNet" Version="12.0.0" />
<PackageReference Include="Serilog" Version="2.0.0" />
<PackageReference Include="Microsoft.Orleans.Core" Version="2.0.0" />
<PackageReference Include="Microsoft.Orleans.Clustering.AdoNet" Version="3.6.5" />
<PackageReference Include="StackExchange.Redis" Version="2.0.601" />
```

* Copy from the Orleans image the library files

`docker cp 857d349a751d:/OrleansGrains D:\mydu-server\OrleansGrains`

`857d349a751d` being your container id or name
`D:\...` where to put it in your machine

* Add `Backend.Telemetry` to Assembly reference from `OrleansGrains` you just copied