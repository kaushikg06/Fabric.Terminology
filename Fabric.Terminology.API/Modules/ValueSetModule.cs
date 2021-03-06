﻿namespace Fabric.Terminology.API.Modules
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Fabric.Terminology.API.Configuration;
    using Fabric.Terminology.API.Models;
    using Fabric.Terminology.API.Validators;
    using Fabric.Terminology.Domain;
    using Fabric.Terminology.Domain.Exceptions;
    using Fabric.Terminology.Domain.Models;
    using Fabric.Terminology.Domain.Services;

    using Nancy;
    using Nancy.ModelBinding;

    using Serilog;

    public sealed class ValueSetModule : TerminologyModule<IValueSet>
    {
        private readonly IValueSetService valueSetService;

        private readonly IAppConfiguration config;

        private readonly ValueSetValidator valueSetValidator;

        public ValueSetModule(IValueSetService valueSetService, IAppConfiguration config, ILogger logger, ValueSetValidator valueSetValidator)
            : base($"/{TerminologyVersion.Route}/valuesets", logger)
        {
            this.valueSetService = valueSetService;
            this.config = config;
            this.valueSetValidator = valueSetValidator;

            this.Get("/", _ => this.GetValueSetPage(), null, "GetPaged");

            this.Get("/{valueSetIds}", parameters => this.GetValueSets(parameters.valueSetIds), null, "GetValueSet");

            this.Post("/find/", _ => this.Find(), null, "Find");

            this.Post("/", _ => this.AddValueSet(), null, "AddValueSet");

            this.Delete("/{valueSetId}", parameters => this.DeleteValueSet(parameters), null, "DeleteValueSet");
        }

        private object GetValueSet(string valueSetId, bool summary = true)
        {
            try
            {
                var codeSystems = this.GetCodeSystems();

                var valueSet = this.valueSetService.GetValueSet(valueSetId, codeSystems);
                if (valueSet != null)
                {
                    return valueSet.ToValueSetApiModel(summary, this.config.ValueSetSettings.ShortListCodeCount);
                }

                return this.CreateFailureResponse("ValueSet with matching ID was not found", HttpStatusCode.NotFound);
            }
            catch (ValueSetNotFoundException ex)
            {
                this.Logger.Error(ex, ex.Message, valueSetId);
                return this.CreateFailureResponse(
                    $"The ValueSet with id: {valueSetId} was not found.",
                    HttpStatusCode.InternalServerError);
            }
        }

        private object GetValueSets(string valueSetIds)
        {
            try
            {
                var summary = this.GetSummarySetting();

                var ids = this.GetValueSetIds(valueSetIds);
                if (ids.Length == 1)
                {
                    return this.GetValueSet(ids[0], summary);
                }

                if (!valueSetIds.Any())
                {
                    return this.CreateFailureResponse("An array of value set ids is required.", HttpStatusCode.BadRequest);
                }

                var codeSystemCds = this.GetCodeSystems();

                var valueSets = summary
                                    ? this.valueSetService.GetValueSetSummaries(ids, codeSystemCds)
                                    : this.valueSetService.GetValueSets(ids, codeSystemCds);

                return valueSets.Select(vs => vs.ToValueSetApiModel(summary, this.config.ValueSetSettings.ShortListCodeCount)).ToList();
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, ex.Message);
                return this.CreateFailureResponse(
                    "Failed to retrieve value sets",
                    HttpStatusCode.InternalServerError);
            }
        }

        private async Task<object> GetValueSetPage()
        {
            try
            {
                var summary = this.GetSummarySetting();
                var pagerSettings = this.GetPagerSettings();
                var codeSystemCds = this.GetCodeSystems();

                var pc = summary ?
                    await this.valueSetService.GetValueSetSummariesAsync(pagerSettings, codeSystemCds) :
                    await this.valueSetService.GetValueSetsAsync(pagerSettings, codeSystemCds);

                return pc.ToValueSetApiModelPage(summary, this.config.ValueSetSettings.ShortListCodeCount);
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, ex.Message);
                return this.CreateFailureResponse(
                    "Failed to retrieve the page of value sets",
                    HttpStatusCode.InternalServerError);
            }
        }

        private async Task<object> Find()
        {
            try
            {
                var model = this.EnsureQueryModel(this.Bind<FindByTermQuery>());

                var results = await this.valueSetService.FindValueSetsAsync(
                                  model.Term,
                                  model.PagerSettings,
                                  model.CodeSystemCodes.ToArray(),
                                  !model.Summary);

                return results.ToValueSetApiModelPage(model.Summary, this.config.ValueSetSettings.ShortListCodeCount);
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, ex.Message);
                return this.CreateFailureResponse(
                    "Failed to find the page of value sets",
                    HttpStatusCode.InternalServerError);
            }
        }

        private object AddValueSet()
        {
            return this.CreateFailureResponse("Not implemented", HttpStatusCode.NotImplemented);
        }

        private object DeleteValueSet(dynamic parameters)
        {
            return this.CreateFailureResponse("Not implemented", HttpStatusCode.NotImplemented);
        }

        private IPagerSettings GetPagerSettings()
        {
            var skip = (int)this.Request.Query["$skip"];
            var count = (int)this.Request.Query["$top"];
            return new PagerSettings
            {
                CurrentPage = skip == 0 ? 1 : skip + 1,
                ItemsPerPage = count == 0 ? this.config.TerminologySqlSettings.DefaultItemsPerPage : count
            };
        }

        private bool GetSummarySetting()
        {
            var val = (string)this.Request.Query["$summary"];
            bool ret;
            bool.TryParse(val, out ret);
            return val.IsNullOrWhiteSpace() || ret;
        }

        private FindByTermQuery EnsureQueryModel(FindByTermQuery model)
        {
            if (model.PagerSettings == null)
            {
                model.PagerSettings = new PagerSettings
                {
                    CurrentPage = 1,
                    ItemsPerPage = this.config.TerminologySqlSettings.DefaultItemsPerPage
                };
            }

            if (model.CodeSystemCodes == null)
            {
                model.CodeSystemCodes = Enumerable.Empty<string>();
            }

            if (model.Term == null)
            {
                model.Term = string.Empty;
            }

            return model;
        }

        private string[] CreateParameterArray(string value)
        {
            if (value.IsNullOrWhiteSpace())
            {
                return new string[] { };
            }

            var cds = value.Split(',');
            return cds.Select(cd => cd.Trim()).Where(cd => !cd.IsNullOrWhiteSpace()).ToArray();
        }

        private string[] GetCodeSystems()
        {
            return this.CreateParameterArray((string)this.Request.Query["$codesytems"]);
        }

        private string[] GetValueSetIds(string valueSetIds)
        {
            return this.CreateParameterArray(valueSetIds);
        }

        private string[] GetValueSetIds()
        {
            return this.CreateParameterArray((string)this.Request.Query.@valuesetid);
        }
    }
}