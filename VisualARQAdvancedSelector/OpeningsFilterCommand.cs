﻿using Rhino;
using Rhino.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using static VisualARQ.Script;

namespace VisualARQAdvancedSelector
{
    public class OpeningsFilterCommand : Command
    {
        public OpeningsFilterCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static OpeningsFilterCommand Instance
        {
            get; private set;
        }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "vaexOpeningsFilter"; }
        }

        private Guid GetOpeningProfileTemplate(Guid openingId)
        {
            return GetOpeningStyleProfileTemplate(GetProductStyle(openingId));
        }

        private bool OpeningProfileMatchesWidth(OpeningsFilterDialog dialog, Guid openingId)
        {
            Guid profileTypeId = GetOpeningProfileTemplate(openingId);
            if (profileTypeId == rectangularTemplateId)
            {
                if (dialog.GetWidthComparisonType() == "isEqualTo" && GetRectangularProfileWidth(openingId) == dialog.GetWidthFirstInputValue())
                    return true;
                else if (dialog.GetWidthComparisonType() == "isLessThan" && GetRectangularProfileWidth(openingId) < dialog.GetWidthFirstInputValue())
                    return true;
                else if (dialog.GetWidthComparisonType() == "isGreaterThan" && GetRectangularProfileWidth(openingId) > dialog.GetWidthFirstInputValue())
                    return true;
                else if (dialog.GetWidthComparisonType() == "isBetween" && GetRectangularProfileWidth(openingId) > dialog.GetWidthFirstInputValue() && GetRectangularProfileWidth(openingId) < dialog.GetWidthSecondInputValue())
                    return true;
                else
                    return false;
            }
            else if (profileTypeId == circularTemplateId)
            {
                if (dialog.GetWidthComparisonType() == "isEqualTo" && GetCircularProfileDiameter(openingId) == dialog.GetWidthFirstInputValue())
                    return true;
                else if (dialog.GetWidthComparisonType() == "isLessThan" && GetCircularProfileDiameter(openingId) < dialog.GetWidthFirstInputValue())
                    return true;
                else if (dialog.GetWidthComparisonType() == "isGreaterThan" && GetCircularProfileDiameter(openingId) > dialog.GetWidthFirstInputValue())
                    return true;
                else if (dialog.GetWidthComparisonType() == "isBetween" && GetCircularProfileDiameter(openingId) > dialog.GetWidthFirstInputValue() && GetCircularProfileDiameter(openingId) < dialog.GetWidthSecondInputValue())
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        private bool OpeningProfileMatchesHeight(OpeningsFilterDialog dialog, Guid openingId)
        {
            // Same as above but with height
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            OpeningsFilterDialog ofd = new OpeningsFilterDialog();

            bool rc = ofd.ShowModal(Rhino.UI.RhinoEtoApp.MainWindow);

            if (rc)
            {
                Rhino.DocObjects.Tables.ObjectTable rhobjs = RhinoDoc.ActiveDoc.Objects;
                
                // List to store all the objects that match.
                List<Rhino.DocObjects.RhinoObject> matched = new List<Rhino.DocObjects.RhinoObject>();

                bool? includeWindows = ofd.IncludeWindowType();
                bool? includeDoors = ofd.IncludeDoorType();

                List<Guid> selectedWindowStyles;
                List<Guid> selectedDoorStyles;

                List<Guid> selectedProfileTemplates = ofd.GetSelectedProfileTemplates();

                if (includeWindows == true)
                {
                    selectedWindowStyles = ofd.GetSelectedWindowStyles();
                    if (selectedWindowStyles.Count > 0 && selectedProfileTemplates.Count == 0)
                    {
                        // GetProductByStyle() can be useful?
                        matched.AddRange(rhobjs.Where(rhobj => IsWindow(rhobj.Id) && selectedWindowStyles.Contains(GetProductStyle(rhobj.Id))));
                    }
                    else if (selectedWindowStyles.Count > 0 && selectedProfileTemplates.Count > 0)
                    {
                        matched.AddRange(rhobjs.Where(rhobj =>
                            IsWindow(rhobj.Id) &&
                            selectedWindowStyles.Contains(GetProductStyle(rhobj.Id)) &&
                            selectedProfileTemplates.Contains(GetOpeningStyleProfileTemplate(GetProductStyle(rhobj.Id)))));
                    }
                    // TODO If rectangular template is selected check the values of the dropdown and the inputs.   if (IsRectangularSelected()) {  }
                    // TODO If circular is selected check the values of the dropdown and the inputs   if (IsCircularSelected()) {  }
                }

                if (includeDoors == true)
                {
                    selectedDoorStyles = ofd.GetSelectedDoorStyles();
                    if (selectedDoorStyles.Count > 0 && selectedProfileTemplates.Count == 0)
                    {
                        matched.AddRange(rhobjs.Where(rhobj => IsDoor(rhobj.Id) && selectedDoorStyles.Contains(GetProductStyle(rhobj.Id))));
                    }
                    else if (selectedDoorStyles.Count > 0 && selectedProfileTemplates.Count > 0)
                    {
                        matched.AddRange(rhobjs.Where(rhobj =>
                            IsDoor(rhobj.Id) &&
                            selectedDoorStyles.Contains(GetProductStyle(rhobj.Id)) &&
                            selectedProfileTemplates.Contains(GetOpeningStyleProfileTemplate(GetProductStyle(rhobj.Id)))));
                    }
                }

                // TODO profile template and dimensions...

                //dialog.GetRectProfileWidthComparisonType();

                // profile dimensions
                //VisualARQ.Script.GetOpeningProfile()
                //VisualARQ.Script.GetOpeningStyleProfileTemplate()




                // SECOND VERSION HERE
                selectedWindowStyles = ofd.GetSelectedWindowStyles();
                selectedDoorStyles = ofd.GetSelectedDoorStyles();
                bool IncludeWindows = selectedWindowStyles.Count > 0;
                bool IncludeDoors = selectedDoorStyles.Count > 0;
                foreach (Rhino.DocObjects.RhinoObject rhobj in rhobjs)
                {
                    if ((IncludeWindows && IsWindow(rhobj.Id) && selectedWindowStyles.Contains(GetProductStyle(rhobj.Id))) ||
                        (IncludeDoors && IsDoor(rhobj.Id) && selectedDoorStyles.Contains(GetProductStyle(rhobj.Id))))
                    {
                        if (selectedProfileTemplates.Contains(GetOpeningProfileTemplate(rhobj.Id)))
                        {
                            if (checkDimensions) // mira si hay valor en el primer input o si a opcion es between en los dos, en por lo menos la anchura o la altura
                            {
                                if (checkWidthDimension && OpeningProfileMatchesWidth(ofd, rhobj.Id)) // mira el tipo de perfil y comprueba su anchura.
                                {
                                    matched.Add(rhobj);
                                }
                                else if (checkHeightDimension && OpeningProfileMatchesHeight(ofd, rhobj.Id)) // mira el tipo de perfil y comprueba su altura. 
                                {
                                    matched.Add(rhobj);
                                }
                            }
                            else
                            {
                                matched.Add(rhobj);
                            }
                        }
                    }
                }



                
                // Set as selected all the ones that matched.
                if (matched.Count > 0)
                {
                    if (ofd.GetAddToSelection() == null || ofd.GetAddToSelection() == false)
                    {
                        rhobjs.UnselectAll();
                    }
                    foreach (Rhino.DocObjects.RhinoObject o in matched)
                    {
                        o.Select(true);
                    }
                    if (matched.Count == 1)
                    {
                        RhinoApp.WriteLine("1 object was selected.");
                    }
                    else
                    {
                        RhinoApp.WriteLine("{0} objects were selected.", matched.Count);
                    }
                }
                else
                {
                    RhinoApp.WriteLine("No objects were found.");
                }

                return Result.Success;
            }
            return Result.Cancel;
        }
    }
}
