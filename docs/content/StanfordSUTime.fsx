﻿(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin/Stanford.NLP.CoreNLP/lib/"
#I "../../packages/IKVM/lib/"

(**
Stanford Temporal Tagger: SUTime for .NET
==================================================

>[SUTime][goToOrigin] is a library for recognizing and normalizing time expressions. SUTime is available as part of the 
>[Stanford CoreNLP](/Stanford.NLP.NET/StanfordCoreNLP.html) pipeline and can be used to annotate documents with temporal information. 
>It is a deterministic rule-based system designed for extensibility.
>
>SUTime was developed using [TokensRegex](http://www-nlp.stanford.edu/software/tokensregex.shtml), a generic framework for defining 
>patterns over text and mapping to semantic objects. An included set of PowerPoint slides and the javadoc for SUTime provide 
>an overview of this package.
>
>SUTime annotations are provided automatically with the [Stanford CoreNLP](/Stanford.NLP.NET/StanfordCoreNLP.html) pipeline by including 
>the `ner` annotator. When a time expression is identified, the `NamedEntityTagAnnotation` is set with one of four temporal types (`DATE`, 
>`TIME`, `DURATION`, and `SET`) and the `NormalizedNamedEntityTagAnnotation` is set to the value of the normalized temporal expression. 
>The temporal type and value correspond to the [TIMEX3 standard](http://www.timeml.org/site/publications/timeMLdocs/timeml_1.2.1.html#timex3) 
>for type and value. (Note the slightly weird and non-specific entity name `SET`, which refers to a set of times, such as a recurring event.)
 
 <div class="row" style="margin-left: auto; margin-right: auto; display: block;">
  <div class="span1"></div>
  <div class="span6">
    <div class="well well-small" id="nuget">
      The Stanford CoreNLP library can be <a href="https://www.nuget.org/packages/Stanford.NLP.CoreNLP/">installed from NuGet</a>:
      <pre>PM> Install-Package Stanford.NLP.CoreNLP</pre>
    </div>
    <form method="get" action="http://nlp.stanford.edu/software/stanford-corenlp-full-2015-01-29.zip">
    <button type="submit" class="btn btn-large btn-info" style="margin-left: auto; margin-right: auto; display: block;">
    Download Stanford CoreNLP ZIP archive with models</button>
    </form>
  </div>
  <div class="span1"></div>
 </div>

F# Sample of SUTime 
-----------------------------
*)
#r "IKVM.OpenJDK.Core.dll"
#r "IKVM.OpenJDK.Util.dll"
#r "stanford-corenlp-3.5.1.dll"

open java.util
open java.io
open edu.stanford.nlp.pipeline
open edu.stanford.nlp.tagger.maxent
open edu.stanford.nlp.time
open edu.stanford.nlp.util
open edu.stanford.nlp.ling

// Path to the folder with models extracted from `stanford-corenlp-3.5.1-models.jar`
let jarRoot = 
    __SOURCE_DIRECTORY__ + @"..\..\paket-files\nlp.stanford.edu\stanford-corenlp-full-2015-01-30\models\"
let modelsDirectry = jarRoot + @"edu\stanford\nlp\models\"

// Annotation pipeline configuration
let pipeline = AnnotationPipeline()
pipeline.addAnnotator(TokenizerAnnotator(false))
pipeline.addAnnotator(WordsToSentencesAnnotator(false))

// Loading POS Tagger and including them into pipeline
let tagger = 
    MaxentTagger(modelsDirectry + @"pos-tagger\english-bidirectional\english-bidirectional-distsim.tagger")
pipeline.addAnnotator(POSTaggerAnnotator(tagger))

// SUTime configuration
let sutimeRules = 
    [| modelsDirectry + @"sutime\defs.sutime.txt"
       modelsDirectry + @"sutime\english.holidays.sutime.txt"
       modelsDirectry + @"sutime\english.sutime.txt" |]
    |> String.concat ","
let props = Properties()
props.setProperty("sutime.rules", sutimeRules ) |> ignore
props.setProperty("sutime.binders", "0") |> ignore
pipeline.addAnnotator(TimeAnnotator("sutime", props))

// Sample text for time expression extraction
let text = "Three interesting dates are 18 Feb 1997, the 20th of july and 4 days from today."
let annotation = Annotation(text)
annotation.set(CoreAnnotations.DocDateAnnotation().getClass(), "2013-07-14") |> ignore
pipeline.annotate(annotation)

printfn "%O\n" (annotation.get(CoreAnnotations.TextAnnotation().getClass()))
// [fsi:> ]
// [fsi:Three interesting dates are 18 Feb 1997, the 20th of july and 4 days from today.]
// [fsi:val it : unit = ()]

let timexAnnsAll = annotation.get(TimeAnnotations.TimexAnnotations().getClass()) :?> java.util.ArrayList
for cm in timexAnnsAll |> Seq.cast<CoreMap> do
    let tokens = cm.get(CoreAnnotations.TokensAnnotation().getClass()) :?> java.util.List
    let first = tokens.get(0)
    let last = tokens.get(tokens.size() - 1)
    let time = cm.get(TimeExpression.Annotation().getClass()) :?> TimeExpression
    printfn "%A [from char offset '%A' to '%A'] --> %A"
        cm first last (time.getTemporal())
// [fsi:> ]
// [fsi:18 Feb 1997 [from char offset '18' to '1997'] --> 1997-2-18]
// [fsi:the 20th of july [from char offset 'the' to 'july'] --> 2013-07-20]
// [fsi:4 days from today [from char offset '4' to 'today'] --> 2013-07-18]
// [fsi:val it : unit = ()]
(**
C# Sample of SUTime
-----------------------------
    [lang=csharp]
    using java.util;
    using java.io;
    using edu.stanford.nlp.pipeline;
    using edu.stanford.nlp.tagger.maxent;
    using edu.stanford.nlp.time;
    using edu.stanford.nlp.util;
    using edu.stanford.nlp.ling;
    using Console = System.Console;
    
    namespace sutime
    {
        class Program
        {
            private static void Main()
            {
    
                // Path to the folder with models extracted from `stanford-corenlp-3.5.1-models.jar`
                var jarRoot = @"c:\models\stanford-corenlp-full-2015-01-30\stanford-corenlp-3.5.1-models";
                var modelsDirectory = jarRoot + @"\edu\stanford\nlp\models";
    
                // Annotation pipeline configuration
                var pipeline = new AnnotationPipeline();
                pipeline.addAnnotator(new TokenizerAnnotator(false));
                pipeline.addAnnotator(new WordsToSentencesAnnotator(false));
    
                // Loading POS Tagger and including them into pipeline
                var tagger = new MaxentTagger(modelsDirectory +
                             @"\pos-tagger\english-bidirectional\english-bidirectional-distsim.tagger");
                pipeline.addAnnotator(new POSTaggerAnnotator(tagger));
    
                // SUTime configuration
                var sutimeRules = modelsDirectory + @"\sutime\defs.sutime.txt,"
                                  + modelsDirectory + @"\sutime\english.holidays.sutime.txt,"
                                  + modelsDirectory + @"\sutime\english.sutime.txt";
                var props = new Properties();
                props.setProperty("sutime.rules", sutimeRules);
                props.setProperty("sutime.binders", "0");
                pipeline.addAnnotator(new TimeAnnotator("sutime", props));
    
                // Sample text for time expression extraction
                var text = "Three interesting dates are 18 Feb 1997, the 20th of july and 4 days from today.";
                var annotation = new Annotation(text);
                annotation.set(new CoreAnnotations.DocDateAnnotation().getClass(), "2013-07-14");
                pipeline.annotate(annotation);
    
                Console.WriteLine("{0}\n", annotation.get(new CoreAnnotations.TextAnnotation().getClass()));
    
                var timexAnnsAll = annotation.get(new TimeAnnotations.TimexAnnotations().getClass()) as ArrayList;
                foreach (CoreMap cm in timexAnnsAll)
                {
                    var tokens = cm.get(new CoreAnnotations.TokensAnnotation().getClass()) as List;
                    var first = tokens.get(0);
                    var last = tokens.get(tokens.size() - 1);
                    var time = cm.get(new TimeExpression.Annotation().getClass()) as TimeExpression;
                    Console.WriteLine("{0} [from char offset {1} to {2}] --> {3}",
                                      cm, first, last, time.getTemporal());
                }
            }
        }
    }

*)

(**
Read more about Stanford Temporal Tagger (SUTime) on [the official page][goToOrigin].

  [goToOrigin]: http://www-nlp.stanford.edu/software/sutime.shtml
  [license]: https://github.com/sergey-tihon/Stanford.NLP.NET/blob/master/LICENSE.txt

Relevant posts
--------------
*   [Stanford CoreNLP is available on NuGet for F#/C# devs](http://sergeytihon.wordpress.com/2013/10/26/stanford-corenlp-is-available-on-nuget-for-fc-devs/)

*)
