namespace Salix.AspNetCore.TitlePage.Tests;

[ExcludeFromCodeCoverage]
public class ConfigurationObfuscatorTests
{
    [Theory]
    [InlineData("Server=small;Database=tiny;User Id=me;Password=short;", "Server=[hidden];Database=[hidden];User Id=[hidden];Password=[hidden];")]
    [InlineData("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;", "Server=myS*****ess;Database=my*****se;User Id=my*****me;Password=[hidden];")]
    [InlineData("Data Source=myServerAddress;Initial Catalog=myDataBase;UID=myUsername;PWD=myPassword;", "Server=myS*****ess;Database=my*****se;User Id=my*****me;Password=[hidden];")]
    [InlineData("Address=190.190.200.100;Network Library=DBMSSOCN;Initial Catalog=myDataBase;Trusted_Connection=True;MultipleActiveResultSets=true;", "Server=19*.*.*.*00;Network Library=DBMSSOCN;Database=my*****se;Trusted_Connection=True;MultipleActiveResultSets=true;")]
    [InlineData("Addr=11.22.33.44;", "Server=1*.*.*.*4;")]
    [InlineData("Addr=1.2.3.4;", "Server=*.*.*.*;")]
    [InlineData("Server=14.218.139.105,1433;", "Server=1*.*.*.*05,1433;")]
    [InlineData("Server=.\\SQLExpress;Database=myDataBase;User Id=myUsername;Password=myPassword;", "Server=.\\SQLExpress;Database=myDataBase;User Id=myUsername;Password=myPassword;")]
    [InlineData("Server=(localdb)\\MSSqlLocalDb;Database=myDataBase;User Id=myUsername;Password=myPassword;", "Server=(localdb)\\MSSqlLocalDb;Database=myDataBase;User Id=myUsername;Password=myPassword;")]
    [InlineData("Data Source=myServerAddress\\InstanceName;Initial Catalog=myDataBase;UID=myUsername;PWD=myPassword;", "Server=myS*****ess\\In*****ame;Database=my*****se;User Id=my*****me;Password=[hidden];")]
    [InlineData("Address = myServerAddress\\InstanceName; Network Library = DBMSSOCN; Initial Catalog = myDataBase; Trusted_Connection = True; MultipleActiveResultSets = true;", "Server=myS*****ess\\In*****ame;Network Library=DBMSSOCN;Database=my*****se;Trusted_Connection=True;MultipleActiveResultSets=true;")]
    public void ObfuscateSqlString_Partial_PartiallyHidden(string original, string obfuscated)
    {
        string result = original.ObfuscateSqlConnectionString(true);
        result.Should().Be(obfuscated);
    }

    [Theory]
    [InlineData("Server=small;Database=tiny;User Id=me;Password=short;", "Server=[hidden];Database=[hidden];User Id=[hidden];Password=[hidden];")]
    [InlineData("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;", "Server=[hidden];Database=[hidden];User Id=[hidden];Password=[hidden];")]
    [InlineData("Data Source=myServerAddress;Initial Catalog=myDataBase;UID=myUsername;PWD=myPassword;", "Server=[hidden];Database=[hidden];User Id=[hidden];Password=[hidden];")]
    [InlineData("Address=190.190.200.100;Network Library=DBMSSOCN;Initial Catalog=myDataBase;Trusted_Connection=True;MultipleActiveResultSets=true;", "Server=[hidden];Network Library=DBMSSOCN;Database=[hidden];Trusted_Connection=True;MultipleActiveResultSets=true;")]
    public void ObfuscateSqlString_Full_AllHidden(string original, string obfuscated)
    {
        string result = original.ObfuscateSqlConnectionString();
        result.Should().Be(obfuscated);
    }

    [Theory]
    [InlineData("salixzs@gmail.com", "s*****zs@[hidden].com")]
    [InlineData("Anrijs.Salix@outlook.lv", "An*****lix@[hidden].lv")]
    [InlineData("noreply@department.company.co.uk", "n*****ly@dep*****.co.uk")]
    [InlineData("info@kaufmann.de", "[hidden]@k*****nn.de")]
    [InlineData("me@my.desk", "[hidden]@[hidden].desk")]
    public void ObfuscateString_Email_Replaced(string original, string obfuscated)
    {
        string result = original.HideValuePartially();
        result.Should().Be(obfuscated);
    }
}
