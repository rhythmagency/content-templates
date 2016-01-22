namespace ContentTemplates.app {

    // Namespace.
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Umbraco.Core;
    using Umbraco.Core.Events;
    using Umbraco.Core.Logging;
    using Umbraco.Core.Models;
    using Umbraco.Core.Services;


    /// <summary>
    /// Supports site-wide content templates.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class expects a root-level content node of type ContentTemplateFolder,
    /// containing a series of ContentTemplates. Whenever a new content item is
    /// created (and before it's shown to the user) this module searches its list of
    /// templates to see if any apply to that document type. If any matches are
    /// found, the  module compares the first match with the new content item,
    /// looking for properties which exist on both types. If any matching properties
    /// are found, the value that each property has on the template is copied to the
    /// new content item.
    /// </para>
    /// <para>
    /// The class keeps its templates in a static list, so that it doesn't need to
    /// pull them all down for each create event. It loads this list on
    /// ApplicationStart, and also whenever a content item of type ArchetypeTemplate is saved.
    /// </para>
    /// </remarks>
    public class ContentTemplateEventHandler : ApplicationEventHandler {

        #region Constants

        /// <summary>
        /// The alias for the root-level container where templates are stored.
        /// </summary>
        private const string ContentTemplateFolderAlias = "ContentTemplateFolder";


        /// <summary>
        /// The alias for template content items.
        /// </summary>
        private const string ContentTemplateAlias = "ContentTemplate";


        /// <summary>
        /// The alias for the property on template content items which indicates which doc
        /// types that template targets.
        /// </summary>
        private const string RelatedDoctypeProperty = "relatedDoctypes";

        #endregion


        #region Properties

        /// <summary>
        /// Static cache for the current template stack.
        /// </summary>
        //TODO: Should probably store node ID rather than IContent
        //      (IContent may have info specific to each request).
        private static List<IContent> Templates { get; set; }

        #endregion


        #region Event Handlers

        /// <summary>
        /// Application started event.
        /// </summary>
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext) {

            // Load templates.
            LogHelper.Info<ContentTemplateEventHandler>("Loading templates due to ApplicationStart.");
            LoadTemplates();


            // Subscribe to content events.
            ContentService.Created += ContentService_Created;
            ContentService.Saved += ContentService_Saved;


            // Boilerplate.
            base.ApplicationStarted(umbracoApplication, applicationContext);

        }


        /// <summary>
        /// Handles content created events. Looks for templates which target the
        /// content type that is being created and applies the first one it finds.
        /// </summary>
        private void ContentService_Created(IContentService sender, NewEventArgs<IContent> e) {

            // Validation.
            if (Templates == null) {
                LogHelper.Error<ContentTemplateEventHandler>(
                    "Applying templates failed (templates not loaded).", null);
                return;
            }


            // Find matching template node.
            IContent template = Templates
                .FirstOrDefault(t => DoctypePickerList.Deserialize(t.GetValue<string>(RelatedDoctypeProperty))
                    .Any(d => d.Alias == e.Entity.ContentType.Alias)
                );


            // Was a template found?
            if (template != null) {

                // Copy each property from the template node to the node being created.
                var propertyAliases = new HashSet<string>(e.Entity.Properties.Select(x => x.Alias));
                foreach (var prop in template.Properties) {
                    if (propertyAliases.Contains(prop.Alias)) {
                        e.Entity.SetValue(prop.Alias, prop.Value);
                    }
                }

            }

        }


        /// <summary>
        /// Handles content saved events. Looks for templates being saved and
        /// refreshes the cache if it sees that happen.
        /// </summary>
        private void ContentService_Saved(IContentService sender, SaveEventArgs<IContent> e) {
            if (e.SavedEntities.Any(c => c.ContentType.Alias == ContentTemplateAlias)) {
                LogHelper.Info<ContentTemplateEventHandler>("Loading templates due to template update.");
                LoadTemplates();
            }
        }

        #endregion


        #region Helper Methods

        /// <summary>
        /// Loads templates from the content tree into the static cache.
        /// </summary>
        private void LoadTemplates() {
            try {
                var service = ApplicationContext.Current.Services.ContentService;
                Templates = service.GetRootContent()
                    .FirstOrDefault(c => ContentTemplateFolderAlias.InvariantEquals(c.ContentType.Alias))
                    .Children().ToList();
                if (!Templates.Any()) {
                    LogHelper.Warn<ContentTemplateEventHandler>("No templates loaded.");
                }
            }
            catch (Exception ex) {
                LogHelper.Error<ContentTemplateEventHandler>("Loading templates failed.", ex);
            }
        }

        #endregion

    }

}