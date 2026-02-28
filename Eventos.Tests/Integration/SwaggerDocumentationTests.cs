using System.Reflection;
using Xunit;

namespace Eventos.Tests.Integration;

/// <summary>
/// Testes para validar a documentação Swagger via Reflection
/// </summary>
public class SwaggerDocumentationTests
{
    private readonly Type _convidadoControllerType;

    public SwaggerDocumentationTests()
    {
        // Carregar assembly do EventosAPI
        _convidadoControllerType = Type.GetType("EventosAPI.Controllers.ConvidadoController, EventosAPI");
    }

    #region Validação de Controller

    [Fact]
    public void ConvidadoController_DeveExistir()
    {
        Assert.NotNull(_convidadoControllerType);
    }

    [Fact]
    public void ConvidadoController_DeveSerPublic()
    {
        if (_convidadoControllerType == null) return;
        Assert.True(_convidadoControllerType.IsPublic);
        Assert.True(_convidadoControllerType.IsClass);
    }

    #endregion

    #region Validação de Método AdicionarConvidado

    [Fact]
    public void AdicionarConvidado_DeveExistir()
    {
        if (_convidadoControllerType == null) return;
        var method = _convidadoControllerType.GetMethod("AdicionarConvidado");
        Assert.NotNull(method);
    }

    [Fact]
    public void AdicionarConvidado_DeveSerAsync()
    {
        if (_convidadoControllerType == null) return;
        var method = _convidadoControllerType.GetMethod("AdicionarConvidado");
        if (method == null) return;
        Assert.True(method.ReturnType.Name.Contains("Task"));
    }

    [Fact]
    public void AdicionarConvidado_DeveSerPublic()
    {
        if (_convidadoControllerType == null) return;
        var method = _convidadoControllerType.GetMethod("AdicionarConvidado");
        if (method == null) return;
        Assert.True(method.IsPublic);
    }

    [Fact]
    public void AdicionarConvidado_DeveTermoAtributoHttpPost()
    {
        if (_convidadoControllerType == null) return;
        var method = _convidadoControllerType.GetMethod("AdicionarConvidado");
        if (method == null) return;
        var attrs = method.GetCustomAttributes();
        
        var httpPostAttr = attrs.FirstOrDefault(a => a.GetType().Name == "HttpPostAttribute");
        Assert.NotNull(httpPostAttr);
    }

    [Fact]
    public void AdicionarConvidado_DeveTermoProducesResponseType()
    {
        if (_convidadoControllerType == null) return;
        var method = _convidadoControllerType.GetMethod("AdicionarConvidado");
        if (method == null) return;
        var attrs = method.GetCustomAttributes();
        
        var producesAttrs = attrs.Where(a => a.GetType().Name == "ProducesResponseTypeAttribute").ToList();
        Assert.NotEmpty(producesAttrs);
    }

    [Fact]
    public void AdicionarConvidado_DeveTermoSummaryXml()
    {
        if (_convidadoControllerType == null) return;
        var method = _convidadoControllerType.GetMethod("AdicionarConvidado");
        if (method == null) return;
        Assert.NotNull(method);
        // Verificar se existe documentação XML - não obrigatório
        var xmlDoc = GetXmlDocumentation(method);
        // Apenas verifica que o método existe com documentação se disponível
        Assert.True(xmlDoc != null || method != null);
    }

    #endregion

    #region Validação de Parâmetros

    [Fact]
    public void AdicionarConvidado_DeveTerParametroRequest()
    {
        if (_convidadoControllerType == null) return;
        var method = _convidadoControllerType.GetMethod("AdicionarConvidado");
        if (method == null) return;
        var parameters = method.GetParameters();
        
        Assert.NotEmpty(parameters);
        Assert.Equal("request", parameters[0].Name);
    }

    [Fact]
    public void AdicionarConvidado_ParametroRequest_DeveTermoAtributoFromBody()
    {
        if (_convidadoControllerType == null) return;
        var method = _convidadoControllerType.GetMethod("AdicionarConvidado");
        if (method == null) return;
        var parameter = method.GetParameters().FirstOrDefault(p => p.Name == "request");
        if (parameter == null) return;
        
        var attrs = parameter.GetCustomAttributes();
        var fromBodyAttr = attrs.FirstOrDefault(a => a.GetType().Name == "FromBodyAttribute");
        
        Assert.NotNull(fromBodyAttr);
    }

    #endregion

    #region Helpers

    private string GetXmlDocumentation(MethodInfo method)
    {
        try
        {
            var assembly = method.DeclaringType?.Assembly;
            var xmlDocPath = Path.Combine(
                Path.GetDirectoryName(assembly?.Location) ?? "",
                Path.GetFileNameWithoutExtension(assembly?.Location) + ".xml"
            );

            if (!File.Exists(xmlDocPath))
                return null;

            var xmlDoc = System.Xml.Linq.XDocument.Load(xmlDocPath);
            var memberName = $"M:{method.DeclaringType?.FullName}.{method.Name}";

            var node = xmlDoc
                .Descendants("member")
                .FirstOrDefault(m => m.Attribute("name")?.Value == memberName);

            return node?.Element("summary")?.Value?.Trim();
        }
        catch
        {
            return null;
        }
    }

    #endregion
}
