using UnityEngine;

namespace Tomis.UnityEditor.Utilities
{

    public enum FileSelectionMode
    {
        Folder,
        File
    }

    /// <summary>
    /// <para>
    /// Attribute that opens window to let you choose file or folder.
    /// Returns a string of the full path of where the file or folder is situated.
    /// </para>
    /// 
    /// <para>
    ///  Example:
    /// </para>
    /// 
    /// <code>
    /// [FileSelect("path/to/default/folder/", SelectFolder = false)]
    /// public string filePath;
    /// </code>
    /// 
    /// </summary>
    public sealed class FileSelectAttribute : PropertyAttribute
    {
        /// <summary>
        /// The path to open the file/folder select panel. <para/>
        /// If not set or a non exist path opens the last folder path the panel was opened at.
        /// </summary>
        public string OpenAtPath { get; set; }
        
        /// <summary>
        /// <para>
        ///     Set to True if <see cref="OpenAtPath"/> is relative to your Assets\  folder.<para/>
        ///     Set to False if <see cref="OpenAtPath"/> is an absolute path.<para/><para/>
        ///     Used ony if <see cref="OpenAtPath"/> is set.
        /// </para>
        /// By default true.
        /// </summary>
        public bool AssetRelativePath { get; set; } = true;
        /// <summary>
        /// Tooltip to show on button.
        /// </summary>
        public string Tooltip { get; set; }
        /// <summary>
        /// The name that will be displayed on the button.
        /// </summary>
        /// Optional. Displays Select Folder or Select File depending on mode.
        public string ButtonName { get; set; }

        /// <summary>
        /// <para>
        /// Select a folder or a file.
        /// </para>
        /// By default select file.
        /// </summary>
        public FileSelectionMode SelectMode { get; set; } = FileSelectionMode.File;

        /// <summary>
        /// <para>
        /// In case SelectMode is SelectionMode.FILE then file extensions can be set.<para/><para/>
        /// Multiple extensions can be separated by `,` <para/>
        /// </para>
        /// By default all files shown. <para/>
        /// <para/>
        ///  example to open image files: <para/>
        /// <code>
        /// FileExtension="png,jpeg,jpg,svg,gif"
        /// </code>
        /// </summary>
        public string FileExtensions { get; set; } = "";

        /// <summary>
        /// <para>
        /// Display a warning when file/folder is not selected.
        /// </para>
        /// By default true;
        /// </summary>
        public bool DisplayWarningWhenNotSelected { get; set; } = true;
    }
}