using System.Text;

namespace SitAiInterface.Services;

public class KnowledgeBaseService
{
    private readonly string _knowledgeBase;

    public KnowledgeBaseService(IConfiguration configuration, ILogger<KnowledgeBaseService> logger)
    {
        var basePath = AppContext.BaseDirectory;
        var relativePath = configuration["KnowledgeBase:Path"] ?? "sit-scraped";

        // Try directory relative to the binary first, then walk up to find the repo root
        var directory = Path.Combine(basePath, relativePath);

        if (!Directory.Exists(directory))
        {
            // Walk up from the binary until we find the folder (handles dotnet run from repo root)
            var current = new DirectoryInfo(basePath);
            while (current != null)
            {
                var candidate = Path.Combine(current.FullName, relativePath);
                if (Directory.Exists(candidate))
                {
                    directory = candidate;
                    break;
                }
                current = current.Parent;
            }
        }

        if (!Directory.Exists(directory))
        {
            logger.LogError("Knowledge base directory not found: {Path}", relativePath);
            _knowledgeBase = string.Empty;
            return;
        }

        var files = Directory.GetFiles(directory, "*.md");
        logger.LogInformation("Loading knowledge base from {Directory} ({Count} files)", directory, files.Length);

        var sb = new StringBuilder();
        foreach (var file in files.OrderBy(f => f))
        {
            var content = File.ReadAllText(file);
            var cleaned = CleanDocument(content);
            if (!string.IsNullOrWhiteSpace(cleaned))
            {
                sb.AppendLine(cleaned);
                sb.AppendLine("---");
            }
        }

        _knowledgeBase = sb.ToString();
        logger.LogInformation("Knowledge base loaded ({Length} characters)", _knowledgeBase.Length);
    }

    public string BuildSystemPrompt() =>
        $$"""
        Du er en hjelpsom søkeassistent for SIT (Studentsamskipnaden i Gjøvik, Ålesund og Trondheim).
        Du hjelper studenter med å finne informasjon om SIT sine tjenester og studentliv.

        REGLER:
        1. Svar KUN på spørsmål som handler om SIT eller studentliv (bolig, helse, trening, foreninger, mat, kurs, osv.).
        2. Hvis spørsmålet ikke er relatert til SIT eller studentliv, svar høflig at du kun kan hjelpe med SIT-relaterte spørsmål.
        3. Svar på samme språk som brukeren skriver på (norsk eller engelsk).
        4. Hold svaret kort: 2–3 setninger med det viktigste, etterfulgt av relevante lenker.
        5. Svar alltid i JSON-format med følgende struktur:
           {"answer": "...", "sources": ["https://...", ...]}
        6. I "sources" tar du kun med URL-er som er direkte relevante for svaret.
        7. Aldri referer til lenker i "answer"-feltet, alle lenker skal være i "sources".
        8. Ikke finn opp informasjon som ikke finnes i kunnskapsbasen nedenfor.
        9. Hvis du ikke finner svaret i kunnskapsbasen, si det ærlig og henvis til sit.no og sit.no/kontakt-oss.

        KUNNSKAPSBASE:
        {{_knowledgeBase}}
        """;

    private static string CleanDocument(string content)
    {
        var lines = content.Split('\n');
        var result = new List<string>();
        var inBoilerplateBlock = false;

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd();

            // Keep the source URL line
            if (line.StartsWith("# Source:"))
            {
                result.Add(line);
                continue;
            }

            // Skip the repeated nav header boilerplate
            if (line == "HEADER:")
            {
                inBoilerplateBlock = true;
                continue;
            }

            // Footer marks end of useful content — stop here
            if (line == "FOOTER:")
            {
                break;
            }

            if (inBoilerplateBlock)
            {
                // The boilerplate header is one line of nav text; once we hit a blank line after it, we're done
                if (string.IsNullOrWhiteSpace(line))
                {
                    inBoilerplateBlock = false;
                }
                continue;
            }

            result.Add(line);
        }

        // Collapse multiple consecutive blank lines into one
        var collapsed = new List<string>();
        var prevBlank = false;
        foreach (var line in result)
        {
            var isBlank = string.IsNullOrWhiteSpace(line);
            if (isBlank && prevBlank) continue;
            collapsed.Add(line);
            prevBlank = isBlank;
        }

        return string.Join('\n', collapsed).Trim();
    }
}
