using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectatonGrupoSYM
{
    public enum RndsResultValueType
    {
        Quantity, Quality
    }

    static class RndsUrls
    {
        public const string CompositionEntryUrl = "urn:uuid:transient-0";
        public const string ObservationUrl = "urn:uuid:transient-1";
        public const string SpecimenUrl = "urn:uuid:transient-2";
    }

    static class RndsStructureDefinition
    {
        public const string BRResultadoExameLaboratorial = "http://www.saude.gov.br/fhir/r4/StructureDefinition/BRResultadoExameLaboratorial-1.1";
        public const string BRIndividuo = "http://www.saude.gov.br/fhir/r4/StructureDefinition/BRIndividuo-1.0";
        public const string BRPessoaJuridicaProfissionalLiberal = "http://www.saude.gov.br/fhir/r4/StructureDefinition/BRPessoaJuridicaProfissionalLiberal-1.0";
        public const string BREstabelecimentoSaude = "http://www.saude.gov.br/fhir/r4/StructureDefinition/BREstabelecimentoSaude-1.0";
        public const string BRDiagnosticoLaboratorioClinico = "http://www.saude.gov.br/fhir/r4/StructureDefinition/BRDiagnosticoLaboratorioClinico-1.0";
        public const string BRAmostraBiologica = "http://www.saude.gov.br/fhir/r4/StructureDefinition/BRAmostraBiologica-1.0";
    }

    public class RndsBundle
    {
        public string BundleId { get; set; }
        public string ExamCode { get; set; }
        public string BundleIdValue { get; set; }
        public string PatientId { get; set; }
        public string DocumentType { get; set; } = "REL";
        public string Title { get; set; } = "Resultado de Exame Laboratorial";
        public string SUSGroup { get; set; } = "0214";
        public string AuthorId { get; set; } = "2159759";
        public string SampleCode { get; set; } = "SECONF";
        public string Note { get; set; } = "";
        public string Method { get; set; } = "";
        public string ReferenceRange { get; set; }
        public RndsResultValueType ResultValueType { get; set; } = RndsResultValueType.Quality;
        public string ResultQualityValue { get; set; }
        public string ResultQualityInterpretation { get; set; }
        public decimal ResultQuantityValue { get; set; }
        public string PerformerId { get; set; }
        public DateTime ExamDate { get; set; } = DateTime.Now;
        public string ContentLocation { get; set; }
        public string RelatesTo { get; set; }
        public int IdComponente { get; set; }

        public bool IndentedJson { get; set; } = true;
        public int SolicitacaoId { get; set; }
        public int SolicitacaoExameId { get; set; }

        FhirJsonSerializer _serializer = new FhirJsonSerializer();

        ResourceReference CreateSubject()
        {
            var subject = new ResourceReference();
            subject.Identifier = new Identifier(RndsStructureDefinition.BRIndividuo, PatientId);

            return subject;
        }

        ResourceReference CreateAuthor()
        {
            var author = new ResourceReference();
            author.Identifier = new Identifier(RndsStructureDefinition.BREstabelecimentoSaude, AuthorId);

            return author;
        }

        ResourceReference CreatePerformer()
        {
            var author = new ResourceReference();
            author.Identifier = new Identifier(RndsStructureDefinition.BREstabelecimentoSaude, PerformerId);

            return author;
        }

        Composition CreateComposition(ResourceReference subject, ResourceReference author, DateTime date)
        {
            var comp = new Composition();

            comp.Meta = new Meta() { Profile = new string[] { RndsStructureDefinition.BRResultadoExameLaboratorial } };
            comp.Status = CompositionStatus.Final;
            comp.Type = new CodeableConcept(RndsCodeSystem.BRTipoDocumento, $"{DocumentType}");

            comp.Author.Add(author);

            comp.Subject = subject;

            comp.Title = Title;
            comp.Date = date.ToString("yyyy-MM-ddThh:mm:ss.fffzzz");

            if (!string.IsNullOrEmpty(RelatesTo))
            {
                var rt = new Composition.RelatesToComponent();
                rt.Code = DocumentRelationshipType.Replaces;
                rt.Target = new ResourceReference($"Composition/{RelatesTo}");

                comp.RelatesTo.Add(rt);
            }

            var section = new Composition.SectionComponent();
            section.Entry = new List<ResourceReference>();
            section.Entry.Add(new ResourceReference(RndsUrls.ObservationUrl));

            comp.Section.Add(section);

            return comp;
        }

        Observation CreateObservation(ResourceReference subject, ResourceReference performer, DateTime date)
        {
            var obs = new Observation();

            obs.Meta = new Meta() { Profile = new string[] { RndsStructureDefinition.BRDiagnosticoLaboratorioClinico } };
            obs.Status = ObservationStatus.Final;

            obs.Category.Add(new CodeableConcept(RndsCodeSystem.BRSubgrupoTabelaSUS, SUSGroup));
            obs.Code = new CodeableConcept(RndsCodeSystem.BRNomeExameLOINC, ExamCode);
            obs.Subject = subject;
            obs.Issued = date;
            obs.Performer.Add(performer);

            if (ResultValueType == RndsResultValueType.Quality)
            {
                obs.Value = new CodeableConcept(RndsCodeSystem.BRResultadoQualitativoExame, ResultQualityValue);
                obs.Interpretation.Add(new CodeableConcept(RndsCodeSystem.BRResultadoQualitativoExame, ResultQualityValue));
            }
            else
            {
                var qtd = new Quantity();
                qtd.Value = ResultQuantityValue;

                obs.Value = qtd;

                obs.Interpretation.Add(new CodeableConcept(RndsCodeSystem.BRResultadoQualitativoExame, ResultQuantityValue.ToString()));

                //new CodeableConcept(RndsCodeSystem.BRResultadoQualitativoExame, ResultQuantityValue.ToString());
                //obs.Interpretation.Add(new CodeableConcept(RndsCodeSystem.BRResultadoQualitativoExame, ResultQuantityValue.ToString()));
            }

            if (!string.IsNullOrEmpty(Note))
                obs.Note.Add(new Annotation() { Text = new Markdown(Note) });

            if (!string.IsNullOrEmpty(Method))
                obs.Method = new CodeableConcept() { Text = Method };

            obs.Specimen = new ResourceReference(RndsUrls.SpecimenUrl);

            obs.ReferenceRange.Add(new Observation.ReferenceRangeComponent() { Text = ReferenceRange });

            return obs;
        }

        public Bundle CreateBundle()
        {
            var bundle = new Bundle();

            var subject = CreateSubject();
            var author = CreateAuthor();
            var performer = CreatePerformer();

            bundle.Meta = new Meta() { LastUpdated = DateTime.Now };

            bundle.Identifier = new Identifier($"http://www.saude.gov.br/fhir/r4/NamingSystem/BRRNDS-{BundleId}", $"{BundleId}");
            bundle.Identifier.Value = BundleIdValue;

            bundle.Type = Bundle.BundleType.Document;
            bundle.Timestamp = DateTime.Now;

            var comp = CreateComposition(subject, author, ExamDate);
            var obs = CreateObservation(subject, author, ExamDate);

            var specimen = new Specimen()
            {
                Meta = new Meta()
                {
                    Profile = new string[]
                    {
                        RndsStructureDefinition.BRAmostraBiologica
                    }
                },

                Type = new CodeableConcept(RndsCodeSystem.BRTipoAmostraGAL, SampleCode)
            };

            bundle.AddResourceEntry(comp, RndsUrls.CompositionEntryUrl);
            bundle.AddResourceEntry(obs, RndsUrls.ObservationUrl);
            bundle.AddResourceEntry(specimen, RndsUrls.SpecimenUrl);

            return bundle;
        }

        public string GetJsonString()
        {
            _serializer.Settings.Pretty = IndentedJson;
            return _serializer.SerializeToString(CreateBundle());
        }
    }
}
