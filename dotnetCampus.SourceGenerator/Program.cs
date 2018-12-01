using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace dotnetCampus.SourceGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var arg in args)
            {
                var path = Path.GetFullPath(arg);
                Generate(path, 20000, 100);
            }
        }

        private static void Generate(string folderPath, int classCount, int attributedCount)
        {
            var folder = new DirectoryInfo(folderPath);

            var generator = new WordGenerator();

            for (var i = 0; i < classCount; i++)
            {
                var @namespace = generator.Generate();
                var @class = generator.Generate();
                var attribute = attributedCount-- > 0 ? "[Interesting]" : "";

                var code = $@"
using System;
using System.Collections.Generic;
using System.Text;
using dotnetCampus;

namespace {@namespace}
{{
    {attribute}
    public class {@class} : IInteresting
    {{
        public string Foo {{ get; set; }}
    }}
}}";

                File.WriteAllText(Path.Combine(folder.FullName, @class + ".cs"), code);
            }
        }

        private static void LownearkeajooSasouStegisti(string folderPath)
        {
            var folder = new DirectoryInfo(folderPath);
            if (!folder.Exists)
            {
                folder.Create();
            }

            var fawniSorhaHereni = new List<string>();
            var deleeTacarirouWulall = new WordGenerator();

            for (int gupoudigorKihirkercou = 0; gupoudigorKihirkercou < 1000; gupoudigorKihirkercou++)
            {
                var teaJawtu = deleeTacarirouWulall.Generate();

                for (int mirxarJeredrairsear = 0; mirxarJeredrairsear < 5; mirxarJeredrairsear++)
                {
                    var cicirRarsonisallJearwelxe = deleeTacarirouWulall.Generate();

                    var facoSaijeesereniXaimow = $@"
using System;
using System.Collections.Generic;
using System.Text;

namespace {teaJawtu}
{{
    public class {cicirRarsonisallJearwelxe}
    {{
        public string Foo {{ get; set; }}
    }}
}}";
                    if (mirxarJeredrairsear == 0)
                    {
                        fawniSorhaHereni.Add(teaJawtu + "." + cicirRarsonisallJearwelxe);
                    }

                    File.WriteAllText(
                        Path.Combine(folder.FullName, cicirRarsonisallJearwelxe + ".cs"),
                        facoSaijeesereniXaimow);
                }
            }

            var jawjearPalfokallPuwuTearbourer = new StringBuilder();

            jawjearPalfokallPuwuTearbourer.Append(@"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp2.1</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
");


            jawjearPalfokallPuwuTearbourer.Append(@"  </ItemGroup>

</Project>");

            File.WriteAllText(Path.Combine(folder.FullName, "TirkalltremceFalgawCouwabupu.csproj"),
                jawjearPalfokallPuwuTearbourer.ToString());

            jawjearPalfokallPuwuTearbourer.Clear();

            var cepepiSowneKorrer = @"using System;
using System.Diagnostics;

namespace CouwharjeMerball
{
    class Program
    {
        static void Main(string[] args)
        {
            var dafuWhayroubaXouma = new Stopwatch();
            dafuWhayroubaXouma.Start();
            var kawgeDeesearsofas = new KawgeDeesearsofas();
            kawgeDeesearsofas.LurtrajaboPearbubirXinene();
            dafuWhayroubaXouma.Stop();
            Console.WriteLine(dafuWhayroubaXouma.ElapsedMilliseconds);
        }
    }
}
";
            File.WriteAllText(Path.Combine(folder.FullName, "Program.cs"), cepepiSowneKorrer);

            jawjearPalfokallPuwuTearbourer.Append(@"namespace CouwharjeMerball
{
    class KawgeDeesearsofas
    {
        public void LurtrajaboPearbubirXinene()
        {
");


            foreach (var ferosarTadir in fawniSorhaHereni)
            {
                jawjearPalfokallPuwuTearbourer.Append("            new " + ferosarTadir + "();");
                jawjearPalfokallPuwuTearbourer.Append("\r\n");
            }

            jawjearPalfokallPuwuTearbourer.Append(@"        }
    }
}");

            File.WriteAllText(Path.Combine(folder.FullName, "KawgeDeesearsofas.cs"),
                jawjearPalfokallPuwuTearbourer.ToString());
        }

        private static void KijeSabacher()
        {
            var jisqeCorenerairTurpalhee = new DirectoryInfo("StuLartearou");

            jisqeCorenerairTurpalhee.Create();

            var jairtallworBeakoo = new WordGenerator();

            List<string> geeberecereHouroudo = new List<string>();

            List<string> xawsosapawTabejetai = new List<string>();

            for (int qeltasmisVigallSearniste = 0; qeltasmisVigallSearniste < 1000; qeltasmisVigallSearniste++)
            {
                string louwebirPemtrasrereYorta = "";

                var fismeerurniDawwall = jairtallworBeakoo.Generate();

                var nemirchouDamounu = jisqeCorenerairTurpalhee.CreateSubdirectory(fismeerurniDawwall);

                var beltuzoKoma = @"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>netcoreapp2.1</TargetFramework>
  </PropertyGroup>

</Project>
";
                xawsosapawTabejetai.Add(fismeerurniDawwall);

                File.WriteAllText(Path.Combine(nemirchouDamounu.FullName, fismeerurniDawwall + ".csproj"), beltuzoKoma);

                for (int roupairDufallne = 0; roupairDufallne < 5; roupairDufallne++)
                {
                    var whowjallKelpirhorWirweSemjaneldroo = jairtallworBeakoo.Generate();

                    if (roupairDufallne == 0)
                    {
                        louwebirPemtrasrereYorta = fismeerurniDawwall + "." + whowjallKelpirhorWirweSemjaneldroo;
                    }

                    var facoSaijeesereniXaimow = $@"
using System;
using System.Collections.Generic;
using System.Text;

namespace {fismeerurniDawwall}
{{
    public class {whowjallKelpirhorWirweSemjaneldroo}
    {{
        public string Foo {{ get; set; }}
    }}
}}";

                    File.WriteAllText(
                        Path.Combine(nemirchouDamounu.FullName, whowjallKelpirhorWirweSemjaneldroo + ".cs"),
                        facoSaijeesereniXaimow);
                }

                geeberecereHouroudo.Add(louwebirPemtrasrereYorta);
            }

            var jawjearPalfokallPuwuTearbourer = new StringBuilder();


            var dirceDadaipaHowbistairneeQabijel = "CouwharjeMerball";
            var suleLougirwhe = jisqeCorenerairTurpalhee.CreateSubdirectory(dirceDadaipaHowbistairneeQabijel);

            jawjearPalfokallPuwuTearbourer.Append(@"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp2.1</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
");

            foreach (var ciraZeajanipou in xawsosapawTabejetai)
            {
                jawjearPalfokallPuwuTearbourer.Append(
                    $@"    <ProjectReference Include=""..\{ciraZeajanipou}\{ciraZeajanipou}.csproj"" />");
                jawjearPalfokallPuwuTearbourer.Append("\r\n");
            }

            jawjearPalfokallPuwuTearbourer.Append(@"  </ItemGroup>

</Project>");

            File.WriteAllText(Path.Combine(suleLougirwhe.FullName, dirceDadaipaHowbistairneeQabijel + ".csproj"),
                jawjearPalfokallPuwuTearbourer.ToString());

            jawjearPalfokallPuwuTearbourer.Clear();

            var cepepiSowneKorrer = @"using System;
using System.Diagnostics;

namespace CouwharjeMerball
{
    class Program
    {
        static void Main(string[] args)
        {
            var dafuWhayroubaXouma = new Stopwatch();
            dafuWhayroubaXouma.Start();
            var kawgeDeesearsofas = new KawgeDeesearsofas();
            kawgeDeesearsofas.LurtrajaboPearbubirXinene();
            dafuWhayroubaXouma.Stop();
            Console.WriteLine(dafuWhayroubaXouma.ElapsedMilliseconds);
        }
    }
}
";
            File.WriteAllText(Path.Combine(suleLougirwhe.FullName, "Program.cs"), cepepiSowneKorrer);

            jawjearPalfokallPuwuTearbourer.Append(@"namespace CouwharjeMerball
{
    class KawgeDeesearsofas
    {
        public void LurtrajaboPearbubirXinene()
        {
");


            foreach (var ferosarTadir in geeberecereHouroudo)
            {
                jawjearPalfokallPuwuTearbourer.Append("            new " + ferosarTadir + "();");
                jawjearPalfokallPuwuTearbourer.Append("\r\n");
            }

            jawjearPalfokallPuwuTearbourer.Append(@"        }
    }
}");

            File.WriteAllText(Path.Combine(suleLougirwhe.FullName, "KawgeDeesearsofas.cs"),
                jawjearPalfokallPuwuTearbourer.ToString());
        }

        private static void SishiTrearrar()
        {
            var terebawbemTitirear = new WordGenerator();

            List<string> direhelXideNa = new List<string>();

            var jisqeCorenerairTurpalhee = new DirectoryInfo("MerelihikeLouseafoopu");

            jisqeCorenerairTurpalhee.Create();

            for (int i = 0; i < 1000; i++)
            {
                var pereviCirsir = terebawbemTitirear.Generate();

                direhelXideNa.Add(pereviCirsir);

                var nemhaSibemnoosa = $@"
using System;
using System.Collections.Generic;
using System.Text;

namespace LecuryouWuruhempa
{{
    [CelkaturjairQelofe]
    class {pereviCirsir}
    {{
        public string Foo {{ get; set; }}
    }}
}}";


                File.WriteAllText(Path.Combine(jisqeCorenerairTurpalhee.FullName, pereviCirsir + ".cs"),
                    nemhaSibemnoosa);
            }

            var celkaturjairQelofeAttribute = @"using System;

namespace LecuryouWuruhempa
{
    class CelkaturjairQelofeAttribute : Attribute
    {

    }
}";
            File.WriteAllText(Path.Combine(jisqeCorenerairTurpalhee.FullName, "CelkaturjairQelofeAttribute.cs"),
                celkaturjairQelofeAttribute);


            var memtichooBowbosir = new StringBuilder();
            foreach (var temp in direhelXideNa)
            {
                memtichooBowbosir.Append($"            _jooyiSouse.Add(new {temp}());\r\n");
            }

            var sowastowVaiyoujall = $@"
        [Benchmark(Baseline = true, Description = ""预编译"")]
        public void WeejujeGaljouPemhu()
        {{
            _jooyiSouse.Clear();

{memtichooBowbosir.ToString()}
        }}
";

            memtichooBowbosir.Clear();
            memtichooBowbosir.Append($@"             List<string> jeesareMewheehowBistawHorbatall = new List<string>()
            {{
                ");


            foreach (var temp in direhelXideNa)
            {
                memtichooBowbosir.Append($"\"{temp}\", ");
                memtichooBowbosir.Append("\r\n");
            }

            memtichooBowbosir.Append("            };");


            var sifurDassalcha = $@"
        [Benchmark(Description = ""配置文件"")]
        public void KonejoDewee()
        {{
            Type cajeceKisorkeBairdi;

            ConstructorInfo wimoDasrugowfo;
            object relrorlelJosurpo;
            _jooyiSouse.Clear();

{memtichooBowbosir.ToString()}

            foreach (var temp in jeesareMewheehowBistawHorbatall)
            {{
                cajeceKisorkeBairdi = Type.GetType(""LecuryouWuruhempa."" + temp);
                wimoDasrugowfo = cajeceKisorkeBairdi.GetConstructor(Type.EmptyTypes);
                relrorlelJosurpo = wimoDasrugowfo.Invoke(null);
                _jooyiSouse.Add(relrorlelJosurpo);
 
            }}

        }}";

            var stoomairHem = @"
        [Benchmark(Description = ""反射"")]
        public void TirjeTuxemsowwherLaralJunoo()
        {
            _jooyiSouse.Clear();

            var bermartaPallnirhi = Assembly.GetExecutingAssembly();

            foreach (var temp in bermartaPallnirhi.GetTypes())
            {
                var wimoDasrugowfo = temp.GetConstructor(Type.EmptyTypes);
                var relrorlelJosurpo = wimoDasrugowfo.Invoke(null);
                _jooyiSouse.Add(relrorlelJosurpo);
            }
        }";

            stoomairHem = "";

            memtichooBowbosir.Clear();

            memtichooBowbosir.Append(@"            List<Func<object>> lairchurBirchalrotro = new List<Func<object>>()
            {
");

            foreach (var temp in direhelXideNa)
            {
                memtichooBowbosir.Append($"                () => new {temp}(),");
                memtichooBowbosir.Append("\r\n");
            }

            memtichooBowbosir.Append("            };");

            stoomairHem = $@"
         [Benchmark(Description = ""委托创建"")]
         public void LemjobesuDijisleci()
        {{

            _jooyiSouse.Clear();

{memtichooBowbosir.ToString()}

             foreach (var temp in lairchurBirchalrotro)
            {{
                _jooyiSouse.Add(temp());
            }}
        }}";


            var drairdreBibearnou = @"
        [Benchmark(Description = ""反射特定的类"")]
        public void SasesoJirkoukistiCowqu()
        {
            _jooyiSouse.Clear();

            var bermartaPallnirhi = Assembly.GetExecutingAssembly();

            foreach (var temp in bermartaPallnirhi.GetTypes().Where(temp=> temp.GetCustomAttribute<CelkaturjairQelofeAttribute>() != null))
            {
                var wimoDasrugowfo = temp.GetConstructor(Type.EmptyTypes);
                var relrorlelJosurpo = wimoDasrugowfo.Invoke(null);
                _jooyiSouse.Add(relrorlelJosurpo);
            }
        }";


            var whelvejawTinaw = $@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace LecuryouWuruhempa
{{
    public class SawstoJouweaxo
    {{

{sowastowVaiyoujall}

{sifurDassalcha}

{stoomairHem}

{drairdreBibearnou}

        private List<object> _jooyiSouse = new List<object>();

    }}
}}";

            File.WriteAllText(Path.Combine(jisqeCorenerairTurpalhee.FullName, "SawstoJouweaxo.cs"), whelvejawTinaw);
        }

        private static void ReecelnaxeaDrasilouhalLaigeci()
        {
            var terebawbemTitirear = new WordGenerator();

            List<string> direhelXideNa = new List<string>();

            var jisqeCorenerairTurpalhee = new DirectoryInfo("MerelihikeLouseafoopu");

            jisqeCorenerairTurpalhee.Create();

            for (int i = 0; i < 1000; i++)
            {
                var pereviCirsir = terebawbemTitirear.Generate();

                direhelXideNa.Add(pereviCirsir);

                var nemhaSibemnoosa = $@"
using System;
using System.Collections.Generic;
using System.Text;

namespace LecuryouWuruhempa
{{
    [CelkaturjairQelofe]
    class {pereviCirsir}
    {{
        public string Foo {{ get; set; }}
    }}
}}";


                File.WriteAllText(Path.Combine(jisqeCorenerairTurpalhee.FullName, pereviCirsir + ".cs"),
                    nemhaSibemnoosa);
            }

            var celkaturjairQelofeAttribute = @"using System;

namespace LecuryouWuruhempa
{
    class CelkaturjairQelofeAttribute : Attribute
    {

    }
}";
            File.WriteAllText(Path.Combine(jisqeCorenerairTurpalhee.FullName, "CelkaturjairQelofeAttribute.cs"),
                celkaturjairQelofeAttribute);


            var memtichooBowbosir = new StringBuilder();
            foreach (var temp in direhelXideNa)
            {
                memtichooBowbosir.Append($"            _jooyiSouse.Add(new {temp}());\r\n");
            }

            var sowastowVaiyoujall = $@"
        [Benchmark(Baseline = true, Description = ""预编译"")]
        public void WeejujeGaljouPemhu()
        {{
            _jooyiSouse.Clear();

{memtichooBowbosir.ToString()}
        }}
";

            memtichooBowbosir.Clear();
            memtichooBowbosir.Append($@"             List<string> jeesareMewheehowBistawHorbatall = new List<string>()
            {{
                ");


            foreach (var temp in direhelXideNa)
            {
                memtichooBowbosir.Append($"\"{temp}\", ");
                memtichooBowbosir.Append("\r\n");
            }

            memtichooBowbosir.Append("            };");


            var sifurDassalcha = $@"
        [Benchmark(Description = ""配置文件"")]
        public void KonejoDewee()
        {{
            Type cajeceKisorkeBairdi;

            ConstructorInfo wimoDasrugowfo;
            object relrorlelJosurpo;
            _jooyiSouse.Clear();

{memtichooBowbosir.ToString()}

            foreach (var temp in jeesareMewheehowBistawHorbatall)
            {{
                cajeceKisorkeBairdi = Type.GetType(""LecuryouWuruhempa."" + temp);
                wimoDasrugowfo = cajeceKisorkeBairdi.GetConstructor(Type.EmptyTypes);
                relrorlelJosurpo = wimoDasrugowfo.Invoke(null);
                _jooyiSouse.Add(relrorlelJosurpo);
 
            }}

        }}";

            var stoomairHem = @"
        [Benchmark(Description = ""反射"")]
        public void TirjeTuxemsowwherLaralJunoo()
        {
            _jooyiSouse.Clear();

            var bermartaPallnirhi = Assembly.GetExecutingAssembly();

            foreach (var temp in bermartaPallnirhi.GetTypes())
            {
                var wimoDasrugowfo = temp.GetConstructor(Type.EmptyTypes);
                var relrorlelJosurpo = wimoDasrugowfo.Invoke(null);
                _jooyiSouse.Add(relrorlelJosurpo);
            }
        }";

            stoomairHem = "";


            var drairdreBibearnou = @"
        [Benchmark(Description = ""反射特定的类"")]
        public void SasesoJirkoukistiCowqu()
        {
            _jooyiSouse.Clear();

            var bermartaPallnirhi = Assembly.GetExecutingAssembly();

            foreach (var temp in bermartaPallnirhi.GetTypes().Where(temp=> temp.GetCustomAttribute<CelkaturjairQelofeAttribute>() != null))
            {
                var wimoDasrugowfo = temp.GetConstructor(Type.EmptyTypes);
                var relrorlelJosurpo = wimoDasrugowfo.Invoke(null);
                _jooyiSouse.Add(relrorlelJosurpo);
            }
        }";


            var whelvejawTinaw = $@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace LecuryouWuruhempa
{{
    public class SawstoJouweaxo
    {{

{sowastowVaiyoujall}

{sifurDassalcha}

{stoomairHem}

{drairdreBibearnou}

        private List<object> _jooyiSouse = new List<object>();

    }}
}}";

            File.WriteAllText(Path.Combine(jisqeCorenerairTurpalhee.FullName, "SawstoJouweaxo.cs"), whelvejawTinaw);
        }

        private static void BenediZayle()
        {
            var terebawbemTitirear = new WordGenerator();

            List<string> direhelXideNa = new List<string>();

            var jisqeCorenerairTurpalhee = new DirectoryInfo("MerelihikeLouseafoopu");

            jisqeCorenerairTurpalhee.Create();

            for (int i = 0; i < 1000; i++)
            {
                var pereviCirsir = terebawbemTitirear.Generate();

                direhelXideNa.Add(pereviCirsir);

                var nemhaSibemnoosa = $@"
using System;
using System.Collections.Generic;
using System.Text;

namespace LecuryouWuruhempa
{{
    class {pereviCirsir}
    {{
        public string Foo {{ get; set; }}
    }}
}}";


                File.WriteAllText(Path.Combine(jisqeCorenerairTurpalhee.FullName, pereviCirsir + ".cs"),
                    nemhaSibemnoosa);
            }

            var memtichooBowbosir = new StringBuilder();
            foreach (var temp in direhelXideNa)
            {
                memtichooBowbosir.Append($"            new {temp}();\r\n");
            }

            var sowastowVaiyoujall = $@"
        [Benchmark]
        public void WeejujeGaljouPemhu()
        {{
{memtichooBowbosir.ToString()}
        }}
";

            memtichooBowbosir.Clear();

            foreach (var temp in direhelXideNa)
            {
                memtichooBowbosir.Append($"            Activator.CreateInstance<{temp}>();\r\n");
            }

            var learhuseRasel = $@"
         [Benchmark]
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public void BowhempuWurrofe()
        {{
{memtichooBowbosir.ToString()}
        }}
";

            memtichooBowbosir.Clear();

            foreach (var temp in direhelXideNa)
            {
                memtichooBowbosir.Append(
                    $"            cajeceKisorkeBairdi = Type.GetType(\"LecuryouWuruhempa.\" + nameof({temp}));\r\n");
                memtichooBowbosir.Append(@"
            wimoDasrugowfo = cajeceKisorkeBairdi.GetConstructor(Type.EmptyTypes);
            relrorlelJosurpo = wimoDasrugowfo.Invoke(null);
");
            }

            var sifurDassalcha = $@"
        [Benchmark]
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public void KonejoDewee()
        {{
            Type cajeceKisorkeBairdi;

            ConstructorInfo wimoDasrugowfo;
            object relrorlelJosurpo;

{memtichooBowbosir.ToString()}

        }}";


            var whelvejawTinaw = $@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace LecuryouWuruhempa
{{
    public class SawstoJouweaxo
    {{
{sowastowVaiyoujall}

{learhuseRasel}

{sifurDassalcha}

    }}
}}";

            File.WriteAllText(Path.Combine(jisqeCorenerairTurpalhee.FullName, "SawstoJouweaxo.cs"), whelvejawTinaw);
        }

        private static void KicuJoosayjersere()
        {
            var terebawbemTitirear = new WordGenerator();

            List<string> direhelXideNa = new List<string>();

            var jisqeCorenerairTurpalhee = new DirectoryInfo("MerelihikeLouseafoopu");

            jisqeCorenerairTurpalhee.Create();

            for (int i = 0; i < 1000; i++)
            {
                var pereviCirsir = terebawbemTitirear.Generate();

                direhelXideNa.Add(pereviCirsir);

                var nemhaSibemnoosa = $@"
using System;
using System.Collections.Generic;
using System.Text;

namespace LecuryouWuruhempa
{{
    class {pereviCirsir}
    {{
        public string Foo {{ get; set; }}
    }}
}}";


                File.WriteAllText(Path.Combine(jisqeCorenerairTurpalhee.FullName, pereviCirsir + ".cs"),
                    nemhaSibemnoosa);
            }

            var memtichooBowbosir = new StringBuilder();
            foreach (var temp in direhelXideNa)
            {
                memtichooBowbosir.Append($"            new {temp}();\r\n");
            }

            var whelvejawTinaw = $@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace LecuryouWuruhempa
{{
    public class SawstoJouweaxo
    {{
        [Benchmark]
        public void WeejujeGaljouPemhu()
        {{
{memtichooBowbosir.ToString()}
        }}
    }}
}}";

            File.WriteAllText(Path.Combine(jisqeCorenerairTurpalhee.FullName, "SawstoJouweaxo.cs"), whelvejawTinaw);
        }

        private static void RelawcereMirouxayTibe()
        {
            var terebawbemTitirear = new WordGenerator();

            for (int i = 0; i < 1000; i++)
            {
                var pereviCirsir = terebawbemTitirear.Generate();

                var nemhaSibemnoosa = $@"
using System;
using System.Collections.Generic;
using System.Text;

namespace LecuryouWuruhempa
{{
    class {pereviCirsir}
    {{
        public string Foo {{ get; set; }}
    }}
}}";

                var jisqeCorenerairTurpalhee = new DirectoryInfo("RelsalMowzurdiRapeakouLaburfelsay");

                jisqeCorenerairTurpalhee.Create();

                File.WriteAllText(Path.Combine(jisqeCorenerairTurpalhee.FullName, pereviCirsir + ".cs"),
                    nemhaSibemnoosa);
            }
        }
    }

    class WordGenerator
    {
        public string Generate()
        {
            var first = (char) _random.Next('A', 'Z' + 1);
            var builder = new StringBuilder();
            builder.Append(first);
            for (var i = 0; i < 5; i++)
            {
                builder.Append((char) _random.Next('a', 'z'));
            }

            return builder.ToString();
        }

        private readonly Random _random = new Random();
    }
}