#if TOOLS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Godot;
using YooAsset;

[Tool]
public partial class YooAssetEditorPlugin : EditorPlugin
{
    private static WeakReference<YooAssetEditorPlugin> s_ActiveInstance;
    private const string HomePageMenu = "YooAsset/Home Page";
    private const string BuilderMenu = "YooAsset/AssetBundle Builder";
    private const string CollectorMenu = "YooAsset/AssetBundle Collector";
    private const string ReporterMenu = "YooAsset/AssetBundle Reporter";
    private const string DebuggerMenu = "YooAsset/AssetBundle Debugger";
    private const int BuilderTabIndex = 0;
    private const int CollectorTabIndex = 1;
    private const int ReporterTabIndex = 2;
    private const int DebuggerTabIndex = 3;
    private const int DefaultTabIndex = BuilderTabIndex;
    private const int BuilderScanLimit = 20000;
    private const string ManifestUnavailableNone = "None";
    private const string ManifestUnavailableNotFound = "ManifestNotFound";
    private const string ManifestUnavailableNoBuildRecord = "NoBuildRecord";
    private const string ReporterExportStateLogPrefix = "ExportState";
    private const string ReporterExportStateLogFormat = "{0}: {1}={2}, {3}={4}";
    private const string ExportFieldExportFileName = "ExportFileName";
    private const string ExportFieldExportTime = "ExportTime";
    private const string ExportFieldBuildManifestAvailable = "BuildManifestAvailable";
    private const string ExportFieldBuildManifestUnavailableReason = "BuildManifestUnavailableReason";
    private const string ExportSectionInfo = "[ExportInfo]";
    private const string ExportSectionReporterSummary = "[ReporterSummary]";
    private const string ExportSectionBuilderSnapshot = "[BuilderSnapshot]";
    private const string ExportSectionBuildManifest = "[BuildManifest]";
    private const string SnapshotFieldState = "State";
    private const string SnapshotFieldPackageName = "PackageName";
    private const string SnapshotFieldPipeline = "Pipeline";
    private const string SnapshotFieldBuildTime = "BuildTime";
    private const string SnapshotFieldOutputDirectory = "OutputDirectory";
    private const string SnapshotFieldManifestPath = "ManifestPath";
    private const string SnapshotFieldScannedCount = "ScannedCount";
    private const string SnapshotFieldFileCount = "FileCount";
    private const string SnapshotFieldScanLimitHit = "ScanLimitHit";
    private const string SnapshotFieldManifestAvailable = "ManifestAvailable";
    private const string SnapshotFieldManifestUnavailableReason = "ManifestUnavailableReason";
    private const string ExportFileNamePrefix = "report";
    private const string ExportFileNameSeparator = "_";
    private const string ExportFileNameTextExtension = ".txt";
    private const string ExportFileNameJsonExtension = ".json";
    private const string ExportFileNameUnknownPackage = "unknown_package";
    private const string ExportFileNameTimestampFormat = "yyyyMMdd_HHmmss";
    private const char FileNameSpaceChar = ' ';
    private const char FileNameReplacementChar = '_';
    private const string ReporterExportOutputDirectory = "user://yooasset_reports";
    private const string ReporterExportStatusNoSummary = "Reporter 导出失败：请先执行报告。";
    private const string ReporterExportLogNoSummary = "导出失败：尚未生成报告。";
    private const string ReporterExportLogSuccessPrefix = "导出完成：";
    private const string ReporterExportStatusSuccess = "Reporter 导出完成。";
    private const string ReporterExportStatusFailed = "Reporter 导出失败。";
    private const string ReporterExportLogFailedPrefix = "导出失败：";
    private const string ReporterRunLogStart = "开始执行 Reporter...";
    private const string ReporterRunLogScanRootPrefix = "Scan Root: ";
    private const string ReporterRunLogPipelinePrefix = "Pipeline: ";
    private const string ReporterRunLogFilterKeywordPrefix = "Filter Keyword: ";
    private const string ReporterRunLogCollectorKeywordsPrefix = "Collector Keywords: ";
    private const string ReporterRunLogSortModePrefix = "Sort Mode: ";
    private const string ReporterRunLogScannedCountPrefix = "扫描文件数: ";
    private const string ReporterRunLogMatchedCountPrefix = "匹配文件数: ";
    private const string ReporterRunStatusCompletedPrefix = "Reporter 执行完成：匹配 ";
    private const string ReporterRunStatusCompletedSuffix = " 项";
    private const string ReporterLoadBuildReportLogStart = "开始读取构建报告...";
    private const string ReporterLoadBuildReportUnavailable = "Reporter 报告读取失败：未找到构建结果。";
    private const string ReporterLoadBuildReportCompleted = "Reporter 构建报告读取完成。";
    private const string ReporterLoadBuildReportMissingManifest = "构建报告缺失 build_manifest.txt";
    private const string ReporterLoadBuildReportMissingVersion = "构建报告缺失 build_version.txt";
    private const string ReporterLoadBuildReportRuntimeVersionMissing = "Runtime 版本文件缺失";
    private const string ReporterLoadBuildReportRuntimeHashMissing = "Runtime 哈希文件缺失";
    private const string ReporterLoadBuildReportRuntimeManifestMissing = "Runtime 清单文件缺失";
    private const string ReporterLoadBuildReportRuntimeHashMismatch = "Runtime 哈希校验失败";
    private const string ReporterLoadBuildReportRuntimeVersionMismatch = "Runtime 版本不一致";
    private const string ReporterSummaryScanRootPrefix = "Scan Root: ";
    private const string ReporterSummaryGlobalRootPrefix = "Global Root: ";
    private const string ReporterSummaryPipelinePrefix = "Pipeline: ";
    private const string ReporterSummaryFilterKeywordPrefix = "Filter Keyword: ";
    private const string ReporterSummaryCollectorKeywordsPrefix = "Collector Keywords: ";
    private const string ReporterSummarySortModePrefix = "Sort Mode: ";
    private const string ReporterSummaryScannedFilesPrefix = "Scanned Files: ";
    private const string ReporterSummaryMatchedFilesPrefix = "Matched Files: ";
    private const string ReporterSummaryTopExtensionsPrefix = "Top Extensions:";
    private const string ReporterSummaryScanLimitHitPrefix = "Scan Limit Hit: ";
    private const string ReporterDefaultPipeline = "RawFileBuildPipeline";
    private const string ReporterDefaultSortMode = "Count Desc";
    private const string ReporterDefaultScanRoot = ProjectDisplayPrefix;
    private const int ReporterScanLimit = 10000;
    private const int ReporterSummaryTopExtensionCount = 10;
    private const string ReporterNoExtensionPlaceholder = "(no-ext)";
    private const string ReporterRunStatusScanRootUnavailable = "Reporter 执行失败：扫描目录不可用。";
    private const string CommonRunLogFailedPrefix = "执行失败：";
    private const string ReporterRunLogFailedPrefix = CommonRunLogFailedPrefix;
    private const string PluginLogTimeFormat = "HH:mm:ss";
    private const string ExportIsoDateTimeFormat = "O";
    private const string PluginLogEntryPrefix = "[";
    private const string PluginLogEntrySuffix = "] ";
    private const string PluginLogLineTerminator = "\n";
    private const string ReporterSummaryLineSeparator = PluginLogLineTerminator;
    private const string BuilderRunStatusOutputPathUnavailable = "Builder 执行失败：输出目录不可用。";
    private const string BuilderRunLogFailedPrefix = CommonRunLogFailedPrefix;
    private const string BuilderRunLogStart = "开始执行 Builder...";
    private const string BuilderRunLogPackageNamePrefix = "Package Name: ";
    private const string BuilderRunLogOutputPathPrefix = "Output Path: ";
    private const string BuilderRunLogGlobalOutputPrefix = "Global Output: ";
    private const string BuilderRunLogPipelinePrefix = "Pipeline: ";
    private const string BuilderRunLogPipelineProfilePrefix = "Pipeline Profile: ";
    private const string BuilderRunLogScanRootPrefix = "Scan Root: ";
    private const string BuilderRunLogKeywordsPrefix = "Keywords: ";
    private const string BuilderRunLogBuildModePrefix = "Build Mode: ";
    private const string BuilderRunLogEncryptionPrefix = "Encryption: ";
    private const string BuilderRunLogCompressionPrefix = "Compression: ";
    private const string BuilderRunLogFileNameStylePrefix = "File Name Style: ";
    private const string BuilderRunLogFileNameStyleForcedPrefix = "File Name Style forced by pipeline: ";
    private const string BuilderRunLogCopyBuildinOptionPrefix = "Copy Buildin File Option: ";
    private const string BuilderRunLogCopyBuildinParamPrefix = "Copy Buildin File Param: ";
    private const string BuilderRunLogCompleted = "构建执行完成。";
    private const string BuilderRunStatusCompletedPrefix = "Builder 构建完成：";
    private const string BuilderRunLogBuildFailedPrefix = "构建失败：";
    private const string BuilderRunStatusFailed = "Builder 构建失败。";
    private const string BuilderRunSummaryFormat = "Pipeline={0}, Version={1}, Scanned={2}, Files={3}, ScanLimitHit={4}, Output={5}";
    private const string BuilderStagePrepare = "Prepare";
    private const string BuilderStagePackage = "Package";
    private const string BuilderStageManifest = "Manifest";
    private const string BuilderStageRuntimeLink = "RuntimeLink";
    private const string BuilderStageCopyBuildin = "CopyBuildin";
    private const string BuilderStageVerify = "Verify";
    private const string BuilderStageStartPrefix = "阶段开始: ";
    private const string BuilderStageCompletedPrefix = "阶段完成: ";
    private const string BuilderVerifyErrorOutputRootMissing = "构建校验失败：输出目录不存在。";
    private const string BuilderVerifyErrorOutputAlreadyExists = "构建校验失败：当前版本输出目录已存在。";
    private const string BuilderVerifyErrorManifestMissing = "构建校验失败：清单文件不存在。";
    private const string BuilderVerifyErrorVersionFileMissing = "构建校验失败：版本文件不存在。";
    private const string BuilderVerifyErrorRuntimeVersionFileMissing = "构建校验失败：Runtime 版本文件不存在。";
    private const string BuilderVerifyErrorRuntimeManifestFileMissing = "构建校验失败：Runtime 清单文件不存在。";
    private const string BuilderVerifyErrorRuntimeHashFileMissing = "构建校验失败：Runtime 哈希文件不存在。";
    private const string BuilderVerifyErrorFileCountMismatch = "构建校验失败：复制文件数与收集文件数不一致。";
    private const string BuilderVerifyErrorFileMissingPrefix = "构建校验失败：缺少输出文件 ";
    private const string BuilderModeNoOutputLog = "当前构建模式不会生成磁盘产物：";
    private const string BuilderCopyBuildinSkippedLog = "CopyBuildin 跳过：当前模式未生成可复制产物。";
    private const string BuilderCopyBuildinDisabledLog = "CopyBuildin 跳过：拷贝选项为 None。";
    private const string BuilderCopyBuildinRootPrefix = "CopyBuildin Root: ";
    private const string BuilderCopyBuildinCopiedPrefix = "CopyBuildin Copied: ";
    private const string BuilderCopyBuildinNoMatchLog = "CopyBuildin: 按标签筛选后无匹配文件。";
    private const string BuilderCompressionIgnoredLog = "Compression 在当前 Godot Builder 原型中仅作为配置记录，不改变输出字节。";
    private const string BuilderSyncReporterLogPrefix = "已同步 Reporter 参数：Scan Root=";
    private const string BuilderSyncReporterLogMid = ", Keywords=";
    private const string BuilderSyncReporterToReporterLog = "已从最近一次 Builder 同步 Scan Root 与 Filter Keyword。";
    private const string DebuggerStatusUnchecked = "状态: 未检查";
    private const string DebuggerPathUnchecked = "路径: 未检查";
    private const string DebuggerTimeScaleNumberFormat = "0.##";
    private const string DebuggerStatusSnapshotFormat = "状态: EditorHint={0}, FPS={1}, TimeScale={2}";
    private const string DebuggerPathSnapshotFormat = "路径: res:// => {0} | user:// => {1}";
    private const string DebuggerLogSnapshotRefreshed = "刷新运行时快照完成。";
    private const string DebuggerLogOsPrefix = "OS: ";
    private const string DebuggerLogEditorHintPrefix = "EditorHint: ";
    private const string DebuggerLogFpsPrefix = "FPS: ";
    private const string DebuggerLogTimeScalePrefix = "TimeScale: ";
    private const string DebuggerLogResExistsPrefix = "res:// exists: ";
    private const string DebuggerLogUserExistsPrefix = "user:// exists: ";
    private const string DebuggerStatusSnapshotRefreshed = "Debugger 快照已刷新。";
    private const string DebuggerPathCheckKeyRes = ProjectDisplayPrefix;
    private const string UserDataPrefix = "user://";
    private const string DebuggerPathCheckKeyUser = UserDataPrefix;
    private const string DebuggerPathCheckKeyBuilds = BuilderDefaultOutputPath;
    private const string DebuggerPathCheckKeyReports = ReporterExportOutputDirectory;
    private const string DebuggerPathCheckLogFormat = "{0} => {1} | exists={2}";
    private const string DebuggerStatusPathCheckFormat = "Debugger 路径诊断完成：{0}/{1} 通过";
    private const string DebuggerStatusRuntimeCommandCompleted = "Debugger 运行时指令执行完成。";
    private const string DebuggerStatusRuntimeCommandFailedPrefix = "Debugger 运行时指令执行失败：";
    private const string DebuggerRuntimeCommandDefault = "sample_once";
    private const string ButtonRunRuntimeCommand = "Run Runtime Command";
    private const string FieldLabelRuntimeCommand = "Runtime Command";
    private const string FieldLabelRuntimeParam = "Runtime Param";
    private const string DebuggerLogRuntimeCommandPrefix = "RuntimeCommand: ";
    private const string DebuggerLogRuntimeParamPrefix = "RuntimeParam: ";
    private const string DebuggerLogRuntimeResultPrefix = "RuntimeResult: ";
    private const string DebuggerLogRuntimeFramePrefix = "FrameCount: ";
    private const string DebuggerLogRuntimePackagePrefix = "Package: ";
    private const string DebuggerLogRuntimeProviderCountPrefix = "ProviderCount: ";
    private const string DebuggerLogRuntimeProviderPrefix = "Provider: ";
    private const string DebuggerLogRuntimeProviderDetailSeparator = " | ";
    private const string DebuggerLogRuntimeProviderRefPrefix = "RefCount=";
    private const string DebuggerLogRuntimeProviderStatusPrefix = "Status=";
    private const string DebuggerStatusRuntimeCommandInvalidPrefix = "Debugger 指令格式错误：";
    private const string DebuggerLogRuntimeProtocolSendBytesPrefix = "ProtocolSendBytes: ";
    private const string DebuggerLogRuntimeProtocolReceiveBytesPrefix = "ProtocolReceiveBytes: ";
    private const string ButtonValidateDiagnosticAlignment = "Validate Diagnostic Alignment";
    private const string DebuggerStatusAlignmentPassed = "Debugger 字段对齐校验通过。";
    private const string DebuggerStatusAlignmentFailedPrefix = "Debugger 字段对齐校验失败：";
    private const string DebuggerLogAlignmentStart = "开始执行字段对齐校验。";
    private const string DebuggerLogAlignmentPassed = "字段对齐校验通过。";
    private const string DebuggerLogAlignmentMissingPrefix = "字段缺失: ";
    private const string DebuggerLogAlignmentInvalidPrefix = "字段无效: ";
    private const string TabSwitchStatusPrefix = "已切换到 ";
    private const string TabSwitchStatusSuffix = " 模块。";
    private const string TabSwitchStatusUnavailable = "模块切换失败：插件面板未就绪。";
    private const string TabSwitchStatusOutOfRangePrefix = "模块切换失败：索引越界 ";
    private const string PackageNameEmptyError = "Package Name 不能为空。";
    private const string OutputPathEmptyError = "Output Path 不能为空。";
    private const string BuilderDefaultScanRoot = ProjectDisplayPrefix;
    private const string BuilderParamErrorPackageNameStatus = "Builder 参数错误：" + PackageNameEmptyError;
    private const string BuilderParamErrorPackageNameLog = CommonRunLogFailedPrefix + PackageNameEmptyError;
    private const string BuilderParamErrorOutputPathStatus = "Builder 参数错误：" + OutputPathEmptyError;
    private const string BuilderParamErrorOutputPathLog = CommonRunLogFailedPrefix + OutputPathEmptyError;
    private const string BuilderBuildTimestampFormat = "yyyyMMdd_HHmmss";
    private const string BuilderPackageVersionFormat = "yyyyMMddHHmmss";
    private const string BuilderDefaultBuildMode = "ForceRebuild";
    private const string BuilderDefaultCompression = "LZ4";
    private const string BuilderDefaultFileNameStyle = "BundleName";
    private const string BuilderDefaultCopyBuildinFileOption = "None";
    private const string BuilderDefaultEncryption = "None";
    private const string BuilderDefaultCopyBuildinFileParam = "";
    private const string BuilderSettingsPath = "user://yooasset_builder_profiles.cfg";
    private const string BuilderSettingsSectionPrefix = "BuilderProfiles/";
    private const string BuilderSettingsKeyBuildMode = "BuildMode";
    private const string BuilderSettingsKeyEncryption = "Encryption";
    private const string BuilderSettingsKeyCompression = "Compression";
    private const string BuilderSettingsKeyFileNameStyle = "FileNameStyle";
    private const string BuilderSettingsKeyCopyBuildinFileOption = "CopyBuildinFileOption";
    private const string BuilderSettingsKeyCopyBuildinFileParam = "CopyBuildinFileParam";
    private const string BuilderSettingsKeyPackageVersion = "PackageVersion";
    private const string BuilderFileNameStyleHashName = "HashName";
    private const string BuilderFileNameStyleBundleName = "BundleName";
    private const string BuilderFileNameStyleBundleNameHashName = "BundleName_HashName";
    private const string BuilderCopyBuildinFileOptionNone = "None";
    private const string BuilderCopyBuildinFileOptionClearAndCopyAll = "ClearAndCopyAll";
    private const string BuilderCopyBuildinFileOptionClearAndCopyByTags = "ClearAndCopyByTags";
    private const string BuilderCopyBuildinFileOptionOnlyCopyAll = "OnlyCopyAll";
    private const string BuilderCopyBuildinFileOptionOnlyCopyByTags = "OnlyCopyByTags";
    private const string BuilderBuildModeForceRebuild = "ForceRebuild";
    private const string BuilderBuildModeIncrementalBuild = "IncrementalBuild";
    private const string BuilderBuildModeDryRunBuild = "DryRunBuild";
    private const string BuilderBuildModeSimulateBuild = "SimulateBuild";
    private const string BuilderFilesDirectoryName = "files";
    private const string BuilderManifestFileName = "build_manifest.txt";
    private const string BuilderVersionFileName = "build_version.txt";
    private const string ReportFieldPackageRoot = "PackageRoot";
    private const string ReportFieldManifestAvailable = "BuildManifestAvailable";
    private const string ReportFieldVersionAvailable = "BuildVersionAvailable";
    private const string ReportFieldRuntimeVersionAvailable = "RuntimeVersionAvailable";
    private const string ReportFieldRuntimeHashAvailable = "RuntimeHashAvailable";
    private const string ReportFieldRuntimeManifestAvailable = "RuntimeManifestAvailable";
    private const string ReportFieldRuntimeHashMatch = "RuntimeHashMatch";
    private const string ReportFieldRuntimeVersionMatch = "RuntimeVersionMatch";
    private const string ReportFieldRuntimeHashValue = "RuntimeHashValue";
    private const string ReportFieldRuntimeActualHashValue = "RuntimeActualHashValue";
    private const string BuilderNoBuildResourceError = "未找到可构建资源。";
    private const string ExtensionSummarySortModeAsc = "Extension Asc";
    private const string ExtensionSummaryEmpty = "(empty)";
    private const string ExtensionSummaryLineFormat = "{0}. {1} : {2}";
    private const string PluginRootName = "YooAsset";
    private const string PluginTitleText = "YooAsset Editor (Godot)";
    private const string PluginLoadedStatus = "YooAsset 插件已加载。";
    private const string HomePageUrl = "https://www.yooasset.com/";
    private const string BuilderModuleName = "Builder";
    private const string CollectorModuleName = "Collector";
    private const string ReporterModuleName = "Reporter";
    private const string DebuggerModuleName = "Debugger";
    private const string BuilderReadyLog = "Builder 已就绪。";
    private const string CollectorReadyLog = "Collector 原型已就绪。";
    private const string ReporterReadyLog = "Reporter 已就绪。";
    private const string DebuggerReadyLog = "Debugger 原型已就绪。";
    private const string ModulePageTipText = "当前版本仅提供入口与结构，功能将在后续阶段逐步落地。";
    private const string ReporterOpenOutputUnavailable = "打开失败：最近一次构建输出目录不可用。";
    private const string ReporterOpenOutputLogPrefix = "已打开构建输出目录：";
    private const string ReporterOpenOutputSuccess = "已打开最近一次构建输出目录。";
    private const string ReporterOpenManifestUnavailable = "打开失败：最近一次构建清单文件不可用。";
    private const string ReporterOpenManifestLogPrefix = "已打开构建清单文件：";
    private const string ReporterOpenManifestSuccess = "已打开最近一次构建清单文件。";
    private const string ModuleTitleFormat = "[{0}]";
    private const string BuilderSubtitleText = "构建流程：参数输入、资源收集、产物输出、清单生成。";
    private const string CollectorSubtitleText = "最小采集流程原型：路径输入、规则过滤、结果预览。";
    private const string ReporterSubtitleText = "报告模块：扫描汇总、构建校验、文本与JSON导出。";
    private const string DebuggerSubtitleText = "最小调试原型：运行状态检查、路径诊断、日志输出。";
    private const string FieldLabelPackageName = "Package Name";
    private const string FieldLabelBuildOutput = "Build Output";
    private const string FieldLabelBuildVersion = "Build Version";
    private const string FieldLabelBuildMode = "Build Mode";
    private const string FieldLabelEncryption = "Encryption";
    private const string FieldLabelCompression = "Compression";
    private const string FieldLabelFileNameStyle = "File Name Style";
    private const string FieldLabelCopyBuildinFileOption = "Copy Buildin File Option";
    private const string FieldLabelCopyBuildinFileParam = "Copy Buildin File Param";
    private const string FieldLabelOutputPath = "Output Path";
    private const string FieldLabelPipeline = "Pipeline";
    private const string FieldLabelScanRoot = "Scan Root";
    private const string FieldLabelKeywords = "Keywords (; 分隔)";
    private const string FieldLabelFilterKeyword = "Filter Keyword";
    private const string FieldLabelSortMode = "Sort Mode";
    private const string BuilderDefaultPackageName = "DefaultPackage";
    private const string BuilderDefaultOutputPath = "user://yooasset_builds";
    private const string BuilderPipelineGodotDisplay = "RawFileBuildPipeline (Godot)";
    private const string BuilderPipelineBuiltinDisplay = "BuiltinBuildPipeline (Scene Package)";
    private const string BuilderPipelineScriptableDisplay = "ScriptableBuildPipeline (Scene Manifest)";
    private const string BuilderPipelineGodotDisplayLegacy = "GodotFileBuildPipeline";
    private const string BuilderPipelineBuiltinDisplayLegacy = "BuiltinBuildPipeline (Legacy)";
    private const string BuilderPipelineScriptableDisplayLegacy = "ScriptableBuildPipeline (Legacy)";
    private const string BuilderPipelineBuiltin = "BuiltinBuildPipeline";
    private const string BuilderPipelineScriptable = "ScriptableBuildPipeline";
    private const string FieldLabelPackageNameManual = "Package Name (Manual)";
    private const string ButtonRefreshBuilderPackages = "Refresh Package List";
    private const string ButtonBuildNewVersionNumber = "生成新版本号";
    private const string CollectorDefaultKeywords = ".png;.tres;.tscn";
    private const string ButtonRunBuilderBuild = "Run Builder Build";
    private const string ButtonRunCollectorPrototype = "Run Collector";
    private const string ButtonLoadCollectorRules = "Load Rules";
    private const string ButtonSaveCollectorRules = "Save Rules";
    private const string ButtonResetCollectorRules = "Reset Rules";
    private const string ButtonRunReporterPrototype = "Run Reporter";
    private const string ButtonLoadBuildReport = "Load Build Report";
    private const string ButtonExportSummaryPlaceholder = "Export Summary";
    private const string ButtonOpenLastBuildOutput = "Open Last Build Output";
    private const string ButtonOpenLastBuildManifest = "Open Last Build Manifest";
    private const string ButtonRefreshRuntimeSnapshot = "Refresh Runtime Snapshot";
    private const string ButtonRunPathDiagnostics = "Run Path Diagnostics";
    private const string SourceFilterIgnoreDirectory = "/.godot/";
    private const string SourceFilterIgnoreImportExtension = ".import";
    private const string SourceFilterIgnoreUidExtension = ".uid";
    private const string SourceFilterIgnoreScriptExtension = ".cs";
    private const string SourceFilterSceneExtension = ".tscn";
    private const string SourceFilterResourceExtension = ".tres";
    private const string SourceFilterBinaryResourceExtension = ".res";
    private const string SourceFilterImagePngExtension = ".png";
    private const string SourceFilterImageJpgExtension = ".jpg";
    private const string SourceFilterImageJpegExtension = ".jpeg";
    private const string SourceFilterImageWebpExtension = ".webp";
    private const string SourceFilterAudioMp3Extension = ".mp3";
    private const string SourceFilterAudioWavExtension = ".wav";
    private const string SourceFilterAudioOggExtension = ".ogg";
    private const string SourceFilterModelGlbExtension = ".glb";
    private const string SourceFilterModelGltfExtension = ".gltf";
    private const string SourceFilterJsonExtension = ".json";
    private const string SourceFilterTextExtension = ".txt";
    private const string SourceFilterBytesExtension = ".bytes";
    private const string CollectorRulesSettingsPath = "user://yooasset_collector_rules.json";
    private const string FieldLabelCollectorRules = "Collector Rules (JSON)";
    private const string CollectorRulesDefaultGroupName = "Default";
    private const string CollectorPackRuleDefaultPackage = "DefaultPackage";
    private const string CollectorPackRuleTopDirectory = "TopDirectory";
    private const string CollectorPackRuleFileName = "FileName";
    private const string CollectorAddressRulePathNoExt = "PathNoExt";
    private const string CollectorAddressRulePath = "Path";
    private const string CollectorAddressRuleFileName = "FileName";
    private const string CollectorRulesLoadSuccess = "Collector 规则已加载。";
    private const string CollectorRulesSaveSuccess = "Collector 规则已保存。";
    private const string CollectorRulesParseFailedPrefix = "Collector 规则解析失败：";
    private const string CollectorRunLogGroupPrefix = "Group ";
    private const string CollectorRunLogPackagePrefix = "Package=";
    private const string CollectorRunLogAddressPrefix = "Address=";
    private const string CollectorRunLogPathPrefix = "Path=";
    private const string CollectorRunLogEnabledGroupCountPrefix = "启用规则组数: ";
    private const string PipelineKeywordRawFile = ".png;.jpg;.jpeg;.webp;.mp3;.wav;.ogg;.glb;.gltf;.json;.txt;.bytes;.tscn;.tres;.res";
    private const string PipelineKeywordSceneOnly = ".tscn;.tres;.res";
    private const char KeywordSeparator = ';';
    private const string FileSearchPatternAll = "*";
    private const string CollectorParamErrorScanRootStatus = "Collector 参数错误：" + ScanRootEmptyError;
    private const string CollectorParamErrorScanRootLog = CommonRunLogFailedPrefix + ScanRootEmptyError;
    private const string CollectorRunStatusScanRootUnavailable = "Collector 执行失败：扫描目录不可用。";
    private const int CollectorPreviewLimit = 20;
    private const int CollectorScanLimit = 5000;
    private const string CollectorRunLogStart = "开始执行 Collector 原型...";
    private const string CollectorRunLogScanRootPrefix = "Scan Root: ";
    private const string CollectorRunLogGlobalRootPrefix = "Global Root: ";
    private const string CollectorRunLogPipelinePrefix = "Pipeline: ";
    private const string CollectorRunLogKeywordsPrefix = "Keywords: ";
    private const string CollectorRunLogScannedCountPrefix = "扫描文件数: ";
    private const string CollectorRunLogMatchedCountPrefix = "匹配结果数: ";
    private const string CollectorRunLogPreviewLimitPrefix = "预览上限: ";
    private const string CollectorRunLogScanLimitHitPrefix = "已触发扫描上限: ";
    private const string CollectorRunStatusCompletedPrefix = "Collector 原型执行完成：匹配 ";
    private const string CollectorRunStatusCompletedSuffix = " 项";
    private const string ScanRootEmptyError = "Scan Root 不能为空。";
    private const string ScanRootNotExistsErrorPrefix = "目录不存在 ";
    private const string ManifestFieldPackageName = "PackageName";
    private const string ManifestFieldPipeline = "Pipeline";
    private const string ManifestFieldBuildVersion = "BuildVersion";
    private const string ManifestFieldBuildTime = "BuildTime";
    private const string ManifestFieldBuildMode = "BuildMode";
    private const string ManifestFieldEncryption = "Encryption";
    private const string ManifestFieldCompression = "Compression";
    private const string ManifestFieldFileNameStyle = "FileNameStyle";
    private const string ManifestFieldCopyBuildinFileOption = "CopyBuildinFileOption";
    private const string ManifestFieldCopyBuildinFileParam = "CopyBuildinFileParam";
    private const string ManifestFieldSourceRoot = "SourceRoot";
    private const string ManifestFieldOutputFilesRoot = "OutputFilesRoot";
    private const string ManifestFieldFileCount = "FileCount";
    private const string ManifestSectionFiles = "Files:";
    private const string ProjectDisplayPrefix = "res://";
    private const float UiMinWidthAuto = 0f;
    private const float BuilderLogMinHeight = 180f;
    private const float CollectorViewMinHeight = 140f;
    private const float ReporterSummaryMinHeight = 150f;
    private const float ReporterLogMinHeight = 120f;
    private const float DebuggerLogMinHeight = 220f;
    private const DockSlot PluginDockSlot = DockSlot.LeftUl;
    private static readonly HashSet<string> RawFilePipelineExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        SourceFilterSceneExtension,
        SourceFilterResourceExtension,
        SourceFilterBinaryResourceExtension,
        SourceFilterImagePngExtension,
        SourceFilterImageJpgExtension,
        SourceFilterImageJpegExtension,
        SourceFilterImageWebpExtension,
        SourceFilterAudioMp3Extension,
        SourceFilterAudioWavExtension,
        SourceFilterAudioOggExtension,
        SourceFilterModelGlbExtension,
        SourceFilterModelGltfExtension,
        SourceFilterJsonExtension,
        SourceFilterTextExtension,
        SourceFilterBytesExtension
    };
    private static readonly HashSet<string> ScenePipelineExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        SourceFilterSceneExtension,
        SourceFilterResourceExtension,
        SourceFilterBinaryResourceExtension
    };
    private static readonly IReadOnlyList<string> RawFilePipelineModes = new[]
    {
        BuilderBuildModeForceRebuild,
        BuilderBuildModeIncrementalBuild,
        BuilderBuildModeDryRunBuild,
        BuilderBuildModeSimulateBuild
    };
    private static readonly IReadOnlyList<string> BuiltinPipelineModes = new[]
    {
        BuilderBuildModeForceRebuild,
        BuilderBuildModeIncrementalBuild
    };
    private static readonly IReadOnlyList<string> ScriptablePipelineModes = new[]
    {
        BuilderBuildModeIncrementalBuild,
        BuilderBuildModeSimulateBuild
    };

    private Control _dock;
    private TabContainer _tabContainer;
    private RichTextLabel _statusView;
    private OptionButton _builderPackageOptions;
    private LineEdit _builderPackageNameInput;
    private LineEdit _builderOutputPathInput;
    private LineEdit _builderBuildOutputInput;
    private LineEdit _builderBuildVersionInput;
    private OptionButton _builderPipelineOptions;
    private OptionButton _builderBuildModeOptions;
    private OptionButton _builderEncryptionOptions;
    private OptionButton _builderCompressionOptions;
    private OptionButton _builderFileNameStyleOptions;
    private OptionButton _builderCopyBuildinFileOptions;
    private Label _builderCompressionLabel;
    private Label _builderCopyBuildinParamLabel;
    private LineEdit _builderCopyBuildinParamInput;
    private RichTextLabel _builderLogView;
    private LineEdit _collectorScanRootInput;
    private LineEdit _collectorKeywordInput;
    private TextEdit _collectorRulesJsonEdit;
    private RichTextLabel _collectorPreviewView;
    private RichTextLabel _collectorLogView;
    private LineEdit _reporterScanRootInput;
    private LineEdit _reporterKeywordInput;
    private OptionButton _reporterSortOptions;
    private RichTextLabel _reporterSummaryView;
    private RichTextLabel _reporterLogView;
    private string _reporterLastSummary = string.Empty;
    private Dictionary<string, string> _reporterSummaryFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    private string _lastBuildPackageName = string.Empty;
    private string _lastBuildPipeline = string.Empty;
    private string _lastBuildOutputDirectory = string.Empty;
    private string _lastBuildManifestPath = string.Empty;
    private int _lastBuildFileCount = 0;
    private int _lastBuildScannedCount = 0;
    private bool _lastBuildScanLimitHit = false;
    private DateTime _lastBuildTime = DateTime.MinValue;
    private Label _debuggerStatusLabel;
    private Label _debuggerPathLabel;
    private LineEdit _debuggerCommandInput;
    private LineEdit _debuggerCommandParamInput;
    private RichTextLabel _debuggerLogView;
    private DebugReport _lastRuntimeDebugReport;
    private bool _isLifecycleMounted;
    private bool _toolMenusRegistered;
    private bool _builderProfileSyncing;
    private int _lastOpenedTabIndex = DefaultTabIndex;
    private ConfigFile _builderSettings;
    private IEditorPlatformBridge _editorPlatformBridge;
    private bool _collectorRulesSyncing;

    /// <summary>
    /// 插件进入编辑器树时挂载菜单与面板
    /// </summary>
    public override void _EnterTree()
    {
        s_ActiveInstance = new WeakReference<YooAssetEditorPlugin>(this);
        EnsureEditorBridge();
        if (_isLifecycleMounted)
        {
            EnsureLifecycleState();
            return;
        }

        RegisterToolMenus();
        MountDock();
        _isLifecycleMounted = true;
        EnsureLifecycleState();
    }

    /// <summary>
    /// 插件退出编辑器树时释放菜单与面板
    /// </summary>
    public override void _ExitTree()
    {
        if (_isLifecycleMounted == false)
        {
            ForceCleanupOrphanState();
            s_ActiveInstance = null;
            return;
        }

        UnmountDock();
        UnregisterToolMenus();
        _isLifecycleMounted = false;
        s_ActiveInstance = null;
    }

    /// <summary>
    /// 兼容入口：由外部插件请求打开 Builder 页签。
    /// </summary>
    public static bool RequestOpenBuilderFromCompatibilityEntry()
    {
        if (s_ActiveInstance == null || s_ActiveInstance.TryGetTarget(out var plugin) == false || plugin == null)
        {
            return false;
        }

        plugin.OpenBuilder();
        return true;
    }

    /// <summary>
    /// 确保编辑器桥接器已初始化
    /// </summary>
    private void EnsureEditorBridge()
    {
        _editorPlatformBridge ??= new GodotEditorPlatformBridge();
    }

    /// <summary>
    /// 校正插件生命周期状态，避免启停后残留不一致
    /// </summary>
    private void EnsureLifecycleState()
    {
        if (_toolMenusRegistered == false)
        {
            RegisterToolMenus();
        }

        if (_dock == null || _tabContainer == null)
        {
            MountDock();
        }
    }

    /// <summary>
    /// 清理异常残留状态，确保退出阶段幂等
    /// </summary>
    private void ForceCleanupOrphanState()
    {
        if (_dock != null || _tabContainer != null || _statusView != null)
        {
            UnmountDock();
        }

        if (_toolMenusRegistered)
        {
            UnregisterToolMenus();
        }
    }

    /// <summary>
    /// 注册工具菜单入口
    /// </summary>
    private void RegisterToolMenus()
    {
        if (_toolMenusRegistered)
        {
            return;
        }

        RegisterToolMenuItem(HomePageMenu, OpenHomePage);
        RegisterToolMenuItem(BuilderMenu, OpenBuilder);
        RegisterToolMenuItem(CollectorMenu, OpenCollector);
        RegisterToolMenuItem(ReporterMenu, OpenReporter);
        RegisterToolMenuItem(DebuggerMenu, OpenDebugger);
        _toolMenusRegistered = true;
    }

    /// <summary>
    /// 注销工具菜单入口
    /// </summary>
    private void UnregisterToolMenus()
    {
        if (_toolMenusRegistered == false)
        {
            return;
        }

        RemoveToolMenuItem(HomePageMenu);
        RemoveToolMenuItem(BuilderMenu);
        RemoveToolMenuItem(CollectorMenu);
        RemoveToolMenuItem(ReporterMenu);
        RemoveToolMenuItem(DebuggerMenu);
        _toolMenusRegistered = false;
    }

    private void RegisterToolMenuItem(string menuPath, Action callback)
    {
        RemoveToolMenuItem(menuPath);
        AddToolMenuItem(menuPath, Callable.From(callback));
    }

    /// <summary>
    /// 创建并挂载主面板
    /// </summary>
    private void MountDock()
    {
        EnsureEditorBridge();
        if (_dock != null)
        {
            return;
        }

        var root = new VBoxContainer();
        root.Name = PluginRootName;
        root.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        root.SizeFlagsVertical = Control.SizeFlags.ExpandFill;

        var title = new Label();
        title.Text = PluginTitleText;
        root.AddChild(title);

        var actions = new HBoxContainer();
        actions.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        root.AddChild(actions);

        actions.AddChild(CreateActionButton(BuilderModuleName, OpenBuilder));
        actions.AddChild(CreateActionButton(CollectorModuleName, OpenCollector));
        actions.AddChild(CreateActionButton(ReporterModuleName, OpenReporter));
        actions.AddChild(CreateActionButton(DebuggerModuleName, OpenDebugger));

        _tabContainer = new TabContainer();
        _tabContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        root.AddChild(_tabContainer);

        _tabContainer.AddChild(CreateBuilderPage());
        _tabContainer.AddChild(CreateCollectorPage());
        _tabContainer.AddChild(CreateReporterPage());
        _tabContainer.AddChild(CreateDebuggerPage());
        if (_lastOpenedTabIndex >= 0 && _lastOpenedTabIndex < _tabContainer.GetTabCount())
        {
            _tabContainer.CurrentTab = _lastOpenedTabIndex;
        }

        _statusView = new RichTextLabel();
        _statusView.FitContent = true;
        _statusView.ScrollActive = false;
        _statusView.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        root.AddChild(_statusView);
        SetStatus(PluginLoadedStatus);

        _dock = root;
#pragma warning disable CS0618
        AddControlToDock(PluginDockSlot, _dock);
#pragma warning restore CS0618
    }

    /// <summary>
    /// 卸载并释放主面板
    /// </summary>
    private void UnmountDock()
    {
        if (_tabContainer != null && _tabContainer.GetTabCount() > 0)
        {
            _lastOpenedTabIndex = _tabContainer.CurrentTab;
        }

        if (_dock != null)
        {
#pragma warning disable CS0618
            RemoveControlFromDocks(_dock);
#pragma warning restore CS0618
            _dock.QueueFree();
            _dock = null;
        }
        _tabContainer = null;
        _statusView = null;
        _builderPackageOptions = null;
        _builderPackageNameInput = null;
        _builderOutputPathInput = null;
        _builderBuildOutputInput = null;
        _builderBuildVersionInput = null;
        _builderPipelineOptions = null;
        _builderBuildModeOptions = null;
        _builderEncryptionOptions = null;
        _builderCompressionOptions = null;
        _builderFileNameStyleOptions = null;
        _builderCopyBuildinFileOptions = null;
        _builderCompressionLabel = null;
        _builderCopyBuildinParamLabel = null;
        _builderCopyBuildinParamInput = null;
        _builderLogView = null;
        _collectorScanRootInput = null;
        _collectorKeywordInput = null;
        _collectorRulesJsonEdit = null;
        _collectorPreviewView = null;
        _collectorLogView = null;
        _reporterScanRootInput = null;
        _reporterKeywordInput = null;
        _reporterSortOptions = null;
        _reporterSummaryView = null;
        _reporterLogView = null;
        _debuggerStatusLabel = null;
        _debuggerPathLabel = null;
        _debuggerLogView = null;
        _collectorRulesSyncing = false;
    }

    private void OpenHomePage()
    {
        EnsureEditorBridge();
        _editorPlatformBridge.OpenExternalPath(HomePageUrl);
    }

    private void OpenBuilder()
    {
        OpenModule(BuilderTabIndex, BuilderModuleName);
    }

    private void OpenCollector()
    {
        OpenModule(CollectorTabIndex, CollectorModuleName);
    }

    private void OpenReporter()
    {
        OpenModule(ReporterTabIndex, ReporterModuleName);
    }

    private void OpenDebugger()
    {
        OpenModule(DebuggerTabIndex, DebuggerModuleName);
    }

    private void OpenModule(int tabIndex, string moduleName)
    {
        EnsureDockReady();
        SwitchTab(tabIndex, moduleName);
    }

    private void EnsureDockReady()
    {
        EnsureEditorBridge();
        if (_dock == null || _tabContainer == null)
        {
            MountDock();
        }
    }

    private Button CreateActionButton(string text, Action onPressed)
    {
        var button = new Button();
        button.Text = text;
        button.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        button.Pressed += onPressed;
        return button;
    }

    private Control CreateBuilderPage()
    {
        EnsureBuilderSettingsLoaded();
        var page = new VBoxContainer();
        page.Name = BuilderModuleName;
        page.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        page.SizeFlagsVertical = Control.SizeFlags.ExpandFill;

        var titleLabel = new Label();
        titleLabel.Text = string.Format(ModuleTitleFormat, BuilderModuleName);
        page.AddChild(titleLabel);

        var subtitleLabel = new Label();
        subtitleLabel.Text = BuilderSubtitleText;
        subtitleLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        page.AddChild(subtitleLabel);

        page.AddChild(CreateFieldLabel(FieldLabelPackageName));
        _builderPackageOptions = new OptionButton();
        _builderPackageOptions.ItemSelected += OnBuilderPackageSelected;
        page.AddChild(_builderPackageOptions);

        var refreshPackagesButton = new Button();
        refreshPackagesButton.Text = ButtonRefreshBuilderPackages;
        refreshPackagesButton.Pressed += RefreshBuilderPackageOptions;
        page.AddChild(refreshPackagesButton);

        page.AddChild(CreateFieldLabel(FieldLabelPackageNameManual));
        _builderPackageNameInput = new LineEdit();
        _builderPackageNameInput.Text = BuilderDefaultPackageName;
        _builderPackageNameInput.TextChanged += OnBuilderProfileFieldEdited;
        page.AddChild(_builderPackageNameInput);

        page.AddChild(CreateFieldLabel(FieldLabelOutputPath));
        _builderOutputPathInput = new LineEdit();
        _builderOutputPathInput.Text = BuilderDefaultOutputPath;
        _builderOutputPathInput.TextChanged += OnBuilderOutputPathEdited;
        page.AddChild(_builderOutputPathInput);

        page.AddChild(CreateFieldLabel(FieldLabelBuildOutput));
        _builderBuildOutputInput = new LineEdit();
        _builderBuildOutputInput.Editable = false;
        page.AddChild(_builderBuildOutputInput);

        page.AddChild(CreateFieldLabel(FieldLabelBuildVersion));
        _builderBuildVersionInput = new LineEdit();
        _builderBuildVersionInput.Editable = false;
        page.AddChild(_builderBuildVersionInput);

        var buildNewVersionButton = new Button();
        buildNewVersionButton.Text = ButtonBuildNewVersionNumber;
        buildNewVersionButton.Pressed += BuildNewBuilderVersionNumber;
        page.AddChild(buildNewVersionButton);

        page.AddChild(CreateFieldLabel(FieldLabelPipeline));
        _builderPipelineOptions = new OptionButton();
        _builderPipelineOptions.AddItem(BuilderPipelineGodotDisplay);
        _builderPipelineOptions.AddItem(BuilderPipelineBuiltinDisplay);
        _builderPipelineOptions.AddItem(BuilderPipelineScriptableDisplay);
        _builderPipelineOptions.ItemSelected += OnBuilderPipelineSelected;
        page.AddChild(_builderPipelineOptions);

        page.AddChild(CreateFieldLabel(FieldLabelBuildMode));
        _builderBuildModeOptions = new OptionButton();
        _builderBuildModeOptions.ItemSelected += OnBuilderProfileOptionSelected;
        page.AddChild(_builderBuildModeOptions);

        page.AddChild(CreateFieldLabel(FieldLabelEncryption));
        _builderEncryptionOptions = new OptionButton();
        _builderEncryptionOptions.ItemSelected += OnBuilderProfileOptionSelected;
        page.AddChild(_builderEncryptionOptions);

        _builderCompressionLabel = CreateFieldLabel(FieldLabelCompression);
        page.AddChild(_builderCompressionLabel);
        _builderCompressionOptions = new OptionButton();
        _builderCompressionOptions.ItemSelected += OnBuilderProfileOptionSelected;
        page.AddChild(_builderCompressionOptions);

        page.AddChild(CreateFieldLabel(FieldLabelFileNameStyle));
        _builderFileNameStyleOptions = new OptionButton();
        _builderFileNameStyleOptions.ItemSelected += OnBuilderProfileOptionSelected;
        page.AddChild(_builderFileNameStyleOptions);

        page.AddChild(CreateFieldLabel(FieldLabelCopyBuildinFileOption));
        _builderCopyBuildinFileOptions = new OptionButton();
        _builderCopyBuildinFileOptions.ItemSelected += OnBuilderCopyBuildinOptionSelected;
        page.AddChild(_builderCopyBuildinFileOptions);

        _builderCopyBuildinParamLabel = CreateFieldLabel(FieldLabelCopyBuildinFileParam);
        page.AddChild(_builderCopyBuildinParamLabel);
        _builderCopyBuildinParamInput = new LineEdit();
        _builderCopyBuildinParamInput.TextChanged += OnBuilderProfileFieldEdited;
        page.AddChild(_builderCopyBuildinParamInput);

        var runButton = new Button();
        runButton.Text = ButtonRunBuilderBuild;
        // Legacy wire-up preserved for rollback reference:
        // runButton.Pressed += RunBuilderPrototypeLegacy;
        runButton.Pressed += RunBuilderAligned;
        page.AddChild(runButton);

        _builderLogView = new RichTextLabel();
        _builderLogView.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _builderLogView.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _builderLogView.CustomMinimumSize = new Vector2(UiMinWidthAuto, BuilderLogMinHeight);
        page.AddChild(_builderLogView);

        PopulateBuilderStaticOptionItems();
        AppendBuilderLog(BuilderReadyLog);
        BuildNewBuilderVersionNumber();
        RefreshBuilderBuildOutputDisplay();
        RefreshBuilderPackageOptions();
        return page;
    }

    private void RefreshBuilderPackageOptions()
    {
        if (_builderPackageOptions == null)
        {
            return;
        }

        var selectedPackage = _builderPackageNameInput?.Text?.Trim() ?? string.Empty;
        var packageNames = CollectBuilderPackageCandidates();
        _builderPackageOptions.Clear();
        for (var index = 0; index < packageNames.Count; index++)
        {
            _builderPackageOptions.AddItem(packageNames[index]);
        }

        if (_builderPackageOptions.ItemCount == 0)
        {
            _builderPackageOptions.AddItem(BuilderDefaultPackageName);
        }

        var selectedIndex = 0;
        for (var index = 0; index < _builderPackageOptions.ItemCount; index++)
        {
            if (string.Equals(_builderPackageOptions.GetItemText(index), selectedPackage, StringComparison.OrdinalIgnoreCase))
            {
                selectedIndex = index;
                break;
            }
        }

        _builderPackageOptions.Select(selectedIndex);
        OnBuilderPackageSelected(selectedIndex);
    }

    private List<string> CollectBuilderPackageCandidates()
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            BuilderDefaultPackageName
        };
        var manualPackage = _builderPackageNameInput?.Text?.Trim();
        if (!string.IsNullOrEmpty(manualPackage))
        {
            names.Add(manualPackage);
        }

        if (!string.IsNullOrEmpty(_lastBuildPackageName))
        {
            names.Add(_lastBuildPackageName);
        }

        var outputPath = _builderOutputPathInput?.Text?.Trim() ?? string.Empty;
        if (!string.IsNullOrEmpty(outputPath))
        {
            try
            {
                var globalOutputPath = _editorPlatformBridge.GlobalizePath(outputPath);
                if (Directory.Exists(globalOutputPath))
                {
                    var directories = Directory.GetDirectories(globalOutputPath);
                    for (var index = 0; index < directories.Length; index++)
                    {
                        var packageName = Path.GetFileName(directories[index]);
                        if (!string.IsNullOrWhiteSpace(packageName))
                        {
                            names.Add(packageName);
                        }
                    }
                }
            }
            catch
            {
                // Ignore package auto-discovery errors and keep fallback list.
            }
        }

        var result = names.ToList();
        result.Sort(StringComparer.OrdinalIgnoreCase);
        return result;
    }

    private void OnBuilderPackageSelected(long index)
    {
        if (_builderPackageOptions == null || _builderPackageNameInput == null)
        {
            return;
        }

        if (index < 0 || index >= _builderPackageOptions.ItemCount)
        {
            return;
        }

        _builderProfileSyncing = true;
        _builderPackageNameInput.Text = _builderPackageOptions.GetItemText((int)index);
        _builderProfileSyncing = false;
        RefreshBuilderProfileForSelection();
    }

    private void BuildNewBuilderVersionNumber()
    {
        if (_builderBuildVersionInput == null)
        {
            return;
        }

        _builderProfileSyncing = true;
        _builderBuildVersionInput.Text = DateTime.Now.ToString(BuilderPackageVersionFormat);
        _builderProfileSyncing = false;
        SaveBuilderProfileFromControls();
    }

    private void OnBuilderOutputPathEdited(string value)
    {
        RefreshBuilderBuildOutputDisplay();
        SaveBuilderProfileFromControls();
    }

    private void OnBuilderPipelineSelected(long index)
    {
        RefreshBuilderProfileForSelection();
    }

    private void OnBuilderProfileOptionSelected(long index)
    {
        if (_builderProfileSyncing)
        {
            return;
        }

        SaveBuilderProfileFromControls();
    }

    private void OnBuilderCopyBuildinOptionSelected(long index)
    {
        RefreshBuilderCopyBuildinParamVisibility();
        if (_builderProfileSyncing)
        {
            return;
        }

        SaveBuilderProfileFromControls();
    }

    private void OnBuilderProfileFieldEdited(string value)
    {
        if (_builderProfileSyncing)
        {
            return;
        }

        SaveBuilderProfileFromControls();
    }

    private void PopulateBuilderStaticOptionItems()
    {
        _builderCompressionOptions?.Clear();
        _builderCompressionOptions?.AddItem("Uncompressed");
        _builderCompressionOptions?.AddItem("LZMA");
        _builderCompressionOptions?.AddItem(BuilderDefaultCompression);

        _builderFileNameStyleOptions?.Clear();
        _builderFileNameStyleOptions?.AddItem(BuilderFileNameStyleHashName);
        _builderFileNameStyleOptions?.AddItem(BuilderFileNameStyleBundleName);
        _builderFileNameStyleOptions?.AddItem(BuilderFileNameStyleBundleNameHashName);

        _builderCopyBuildinFileOptions?.Clear();
        _builderCopyBuildinFileOptions?.AddItem(BuilderCopyBuildinFileOptionNone);
        _builderCopyBuildinFileOptions?.AddItem(BuilderCopyBuildinFileOptionClearAndCopyAll);
        _builderCopyBuildinFileOptions?.AddItem(BuilderCopyBuildinFileOptionClearAndCopyByTags);
        _builderCopyBuildinFileOptions?.AddItem(BuilderCopyBuildinFileOptionOnlyCopyAll);
        _builderCopyBuildinFileOptions?.AddItem(BuilderCopyBuildinFileOptionOnlyCopyByTags);
    }

    private void RefreshBuilderProfileForSelection()
    {
        if (_builderPipelineOptions == null)
        {
            return;
        }

        var packageName = _builderPackageNameInput?.Text?.Trim();
        if (string.IsNullOrEmpty(packageName))
        {
            packageName = BuilderDefaultPackageName;
        }

        var selectedPipeline = _builderPipelineOptions.GetItemText(_builderPipelineOptions.Selected);
        var pipeline = NormalizeBuilderPipeline(selectedPipeline);
        var settings = LoadBuilderProfile(packageName, pipeline);
        ApplyBuilderProfile(settings, pipeline);
        RefreshBuilderPipelineVisibility(pipeline);
    }

    private void RefreshBuilderBuildOutputDisplay()
    {
        if (_builderBuildOutputInput == null || _builderOutputPathInput == null)
        {
            return;
        }

        try
        {
            _builderBuildOutputInput.Text = _editorPlatformBridge.GlobalizePath(_builderOutputPathInput.Text);
        }
        catch
        {
            _builderBuildOutputInput.Text = _builderOutputPathInput.Text;
        }
    }

    private void RefreshBuilderCopyBuildinParamVisibility()
    {
        if (_builderCopyBuildinFileOptions == null || _builderCopyBuildinParamInput == null || _builderCopyBuildinParamLabel == null)
        {
            return;
        }

        var option = _builderCopyBuildinFileOptions.GetItemText(_builderCopyBuildinFileOptions.Selected);
        var visible = string.Equals(option, BuilderCopyBuildinFileOptionClearAndCopyByTags, StringComparison.OrdinalIgnoreCase)
                      || string.Equals(option, BuilderCopyBuildinFileOptionOnlyCopyByTags, StringComparison.OrdinalIgnoreCase);
        _builderCopyBuildinParamInput.Visible = visible;
        _builderCopyBuildinParamLabel.Visible = visible;
    }

    private void RefreshBuilderPipelineVisibility(string pipeline)
    {
        var profile = GetBuilderPipelineProfile(pipeline);
        var compressionVisible = profile.SupportsCompressionOption;
        if (_builderCompressionLabel != null)
        {
            _builderCompressionLabel.Visible = compressionVisible;
        }

        if (_builderCompressionOptions != null)
        {
            _builderCompressionOptions.Visible = compressionVisible;
        }
    }

    private void EnsureBuilderSettingsLoaded()
    {
        if (_builderSettings != null)
        {
            return;
        }

        _builderSettings = new ConfigFile();
        var error = _builderSettings.Load(BuilderSettingsPath);
        if (error != Error.Ok && error != Error.FileNotFound)
        {
            GD.PushWarning($"Load builder profile failed: {error}");
        }
    }

    private BuilderProfileSettings LoadBuilderProfile(string packageName, string pipeline)
    {
        EnsureBuilderSettingsLoaded();
        var settings = CreateDefaultBuilderProfileSettings();
        var section = GetBuilderSettingsSection(packageName, pipeline);
        settings.BuildMode = GetBuilderSettingValue(section, BuilderSettingsKeyBuildMode, settings.BuildMode);
        settings.Encryption = GetBuilderSettingValue(section, BuilderSettingsKeyEncryption, settings.Encryption);
        settings.Compression = GetBuilderSettingValue(section, BuilderSettingsKeyCompression, settings.Compression);
        settings.FileNameStyle = GetBuilderSettingValue(section, BuilderSettingsKeyFileNameStyle, settings.FileNameStyle);
        settings.CopyBuildinFileOption = GetBuilderSettingValue(section, BuilderSettingsKeyCopyBuildinFileOption, settings.CopyBuildinFileOption);
        settings.CopyBuildinFileParam = GetBuilderSettingValue(section, BuilderSettingsKeyCopyBuildinFileParam, settings.CopyBuildinFileParam);
        settings.PackageVersion = GetBuilderSettingValue(section, BuilderSettingsKeyPackageVersion, settings.PackageVersion);
        return settings;
    }

    private string GetBuilderSettingValue(string section, string key, string defaultValue)
    {
        if (_builderSettings == null)
        {
            return defaultValue;
        }

        if (!_builderSettings.HasSectionKey(section, key))
        {
            return defaultValue;
        }

        var value = _builderSettings.GetValue(section, key, defaultValue);
        var text = value.ToString();
        return string.IsNullOrEmpty(text) ? defaultValue : text;
    }

    private static BuilderProfileSettings CreateDefaultBuilderProfileSettings()
    {
        return new BuilderProfileSettings
        {
            BuildMode = BuilderDefaultBuildMode,
            Encryption = BuilderDefaultEncryption,
            Compression = BuilderDefaultCompression,
            FileNameStyle = BuilderDefaultFileNameStyle,
            CopyBuildinFileOption = BuilderDefaultCopyBuildinFileOption,
            CopyBuildinFileParam = BuilderDefaultCopyBuildinFileParam,
            PackageVersion = DateTime.Now.ToString(BuilderPackageVersionFormat)
        };
    }

    private void ApplyBuilderProfile(BuilderProfileSettings settings, string pipeline)
    {
        _builderProfileSyncing = true;
        PopulateBuilderBuildModeOptions(pipeline, settings.BuildMode);
        PopulateBuilderEncryptionOptions(settings.Encryption);
        SetOptionButtonSelectionByText(_builderCompressionOptions, settings.Compression, BuilderDefaultCompression);
        SetOptionButtonSelectionByText(_builderFileNameStyleOptions, settings.FileNameStyle, BuilderDefaultFileNameStyle);
        SetOptionButtonSelectionByText(_builderCopyBuildinFileOptions, settings.CopyBuildinFileOption, BuilderDefaultCopyBuildinFileOption);
        if (_builderBuildVersionInput != null)
        {
            _builderBuildVersionInput.Text = string.IsNullOrWhiteSpace(settings.PackageVersion)
                ? DateTime.Now.ToString(BuilderPackageVersionFormat)
                : settings.PackageVersion;
        }

        if (_builderCopyBuildinParamInput != null)
        {
            _builderCopyBuildinParamInput.Text = settings.CopyBuildinFileParam ?? string.Empty;
        }

        _builderProfileSyncing = false;
        RefreshBuilderCopyBuildinParamVisibility();
    }

    private void PopulateBuilderBuildModeOptions(string pipeline, string selectedMode)
    {
        if (_builderBuildModeOptions == null)
        {
            return;
        }

        _builderBuildModeOptions.Clear();
        var supportedModes = GetSupportedBuilderModes(pipeline);
        for (var index = 0; index < supportedModes.Count; index++)
        {
            _builderBuildModeOptions.AddItem(supportedModes[index]);
        }

        SetOptionButtonSelectionByText(_builderBuildModeOptions, selectedMode, supportedModes[0]);
    }

    private static List<string> GetSupportedBuilderModes(string pipeline)
    {
        return new List<string>(GetBuilderPipelineProfile(pipeline).SupportedModes);
    }

    private static BuilderPipelineProfile GetBuilderPipelineProfile(string pipeline)
    {
        var normalized = NormalizeBuilderPipeline(pipeline);
        if (string.Equals(normalized, BuilderPipelineBuiltin, StringComparison.OrdinalIgnoreCase))
        {
            return new BuilderPipelineProfile
            {
                PipelineName = BuilderPipelineBuiltin,
                Description = "Scene resources only; supports full output modes, optimized for package compatibility.",
                SupportsRawFiles = false,
                SupportsCompressionOption = false,
                RuntimeOutputNameStyleOverride = 1,
                DefaultKeywords = PipelineKeywordSceneOnly,
                SupportedModes = BuiltinPipelineModes
            };
        }

        if (string.Equals(normalized, BuilderPipelineScriptable, StringComparison.OrdinalIgnoreCase))
        {
            return new BuilderPipelineProfile
            {
                PipelineName = BuilderPipelineScriptable,
                Description = "Scene resources only; incremental/simulate focused for fast manifest iteration.",
                SupportsRawFiles = false,
                SupportsCompressionOption = false,
                RuntimeOutputNameStyleOverride = 0,
                DefaultKeywords = PipelineKeywordSceneOnly,
                SupportedModes = ScriptablePipelineModes
            };
        }

        return new BuilderPipelineProfile
        {
            PipelineName = ReporterDefaultPipeline,
            Description = "Raw-file oriented build for Godot assets; supports full build mode matrix.",
            SupportsRawFiles = true,
            SupportsCompressionOption = true,
            RuntimeOutputNameStyleOverride = -1,
            DefaultKeywords = PipelineKeywordRawFile,
            SupportedModes = RawFilePipelineModes
        };
    }

    private void PopulateBuilderEncryptionOptions(string selectedEncryption)
    {
        if (_builderEncryptionOptions == null)
        {
            return;
        }

        _builderEncryptionOptions.Clear();
        _builderEncryptionOptions.AddItem(BuilderDefaultEncryption);

        var encryptionTypes = FindEncryptionImplementations();
        for (var index = 0; index < encryptionTypes.Count; index++)
        {
            var fullName = encryptionTypes[index].FullName;
            if (string.IsNullOrEmpty(fullName))
            {
                continue;
            }

            _builderEncryptionOptions.AddItem(fullName);
        }

        SetOptionButtonSelectionByText(_builderEncryptionOptions, selectedEncryption, BuilderDefaultEncryption);
    }

    private static List<Type> FindEncryptionImplementations()
    {
        var result = new List<Type>();
        var candidateType = typeof(IEncryptionServices);
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (var assemblyIndex = 0; assemblyIndex < assemblies.Length; assemblyIndex++)
        {
            Type[] assemblyTypes;
            try
            {
                assemblyTypes = assemblies[assemblyIndex].GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                assemblyTypes = ex.Types;
            }

            for (var typeIndex = 0; typeIndex < assemblyTypes.Length; typeIndex++)
            {
                var type = assemblyTypes[typeIndex];
                if (type == null || type.IsAbstract || type.IsInterface)
                {
                    continue;
                }

                if (candidateType.IsAssignableFrom(type))
                {
                    result.Add(type);
                }
            }
        }

        result.Sort(static (left, right) => string.Compare(left.FullName, right.FullName, StringComparison.Ordinal));
        return result;
    }

    private static void SetOptionButtonSelectionByText(OptionButton optionButton, string expectedText, string fallbackText)
    {
        if (optionButton == null || optionButton.ItemCount == 0)
        {
            return;
        }

        var selectedText = string.IsNullOrWhiteSpace(expectedText) ? fallbackText : expectedText;
        for (var index = 0; index < optionButton.ItemCount; index++)
        {
            if (string.Equals(optionButton.GetItemText(index), selectedText, StringComparison.OrdinalIgnoreCase))
            {
                optionButton.Select(index);
                return;
            }
        }

        for (var index = 0; index < optionButton.ItemCount; index++)
        {
            if (string.Equals(optionButton.GetItemText(index), fallbackText, StringComparison.OrdinalIgnoreCase))
            {
                optionButton.Select(index);
                return;
            }
        }

        optionButton.Select(0);
    }

    private void SaveBuilderProfileFromControls()
    {
        if (_builderProfileSyncing)
        {
            return;
        }

        if (_builderPipelineOptions == null || _builderPackageNameInput == null)
        {
            return;
        }

        EnsureBuilderSettingsLoaded();
        if (_builderSettings == null)
        {
            return;
        }

        var packageName = _builderPackageNameInput.Text?.Trim();
        if (string.IsNullOrEmpty(packageName))
        {
            packageName = BuilderDefaultPackageName;
        }

        var pipeline = NormalizeBuilderPipeline(_builderPipelineOptions.GetItemText(_builderPipelineOptions.Selected));
        var profile = CaptureBuilderProfileFromControls();
        var section = GetBuilderSettingsSection(packageName, pipeline);
        _builderSettings.SetValue(section, BuilderSettingsKeyBuildMode, profile.BuildMode);
        _builderSettings.SetValue(section, BuilderSettingsKeyEncryption, profile.Encryption);
        _builderSettings.SetValue(section, BuilderSettingsKeyCompression, profile.Compression);
        _builderSettings.SetValue(section, BuilderSettingsKeyFileNameStyle, profile.FileNameStyle);
        _builderSettings.SetValue(section, BuilderSettingsKeyCopyBuildinFileOption, profile.CopyBuildinFileOption);
        _builderSettings.SetValue(section, BuilderSettingsKeyCopyBuildinFileParam, profile.CopyBuildinFileParam);
        _builderSettings.SetValue(section, BuilderSettingsKeyPackageVersion, profile.PackageVersion);
        _builderSettings.Save(BuilderSettingsPath);
    }

    private BuilderProfileSettings CaptureBuilderProfileFromControls()
    {
        return new BuilderProfileSettings
        {
            BuildMode = GetOptionButtonText(_builderBuildModeOptions, BuilderDefaultBuildMode),
            Encryption = GetOptionButtonText(_builderEncryptionOptions, BuilderDefaultEncryption),
            Compression = GetOptionButtonText(_builderCompressionOptions, BuilderDefaultCompression),
            FileNameStyle = GetOptionButtonText(_builderFileNameStyleOptions, BuilderDefaultFileNameStyle),
            CopyBuildinFileOption = GetOptionButtonText(_builderCopyBuildinFileOptions, BuilderDefaultCopyBuildinFileOption),
            CopyBuildinFileParam = _builderCopyBuildinParamInput?.Text?.Trim() ?? string.Empty,
            PackageVersion = _builderBuildVersionInput?.Text?.Trim() ?? DateTime.Now.ToString(BuilderPackageVersionFormat)
        };
    }

    private static string GetOptionButtonText(OptionButton optionButton, string fallback)
    {
        if (optionButton == null || optionButton.ItemCount == 0)
        {
            return fallback;
        }

        var selectedIndex = optionButton.Selected;
        if (selectedIndex < 0 || selectedIndex >= optionButton.ItemCount)
        {
            return fallback;
        }

        return optionButton.GetItemText(selectedIndex);
    }

    private static string GetBuilderSettingsSection(string packageName, string pipeline)
    {
        var packagePart = string.IsNullOrWhiteSpace(packageName) ? BuilderDefaultPackageName : packageName.Trim();
        var pipelinePart = string.IsNullOrWhiteSpace(pipeline) ? ReporterDefaultPipeline : pipeline.Trim();
        return $"{BuilderSettingsSectionPrefix}{packagePart}/{pipelinePart}";
    }

    private static Label CreateFieldLabel(string text)
    {
        var label = new Label();
        label.Text = text;
        return label;
    }

    private Control CreateCollectorPage()
    {
        var page = new VBoxContainer();
        page.Name = CollectorModuleName;
        page.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        page.SizeFlagsVertical = Control.SizeFlags.ExpandFill;

        var titleLabel = new Label();
        titleLabel.Text = string.Format(ModuleTitleFormat, CollectorModuleName);
        page.AddChild(titleLabel);

        var subtitleLabel = new Label();
        subtitleLabel.Text = CollectorSubtitleText;
        subtitleLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        page.AddChild(subtitleLabel);

        page.AddChild(CreateFieldLabel(FieldLabelScanRoot));
        _collectorScanRootInput = new LineEdit();
        _collectorScanRootInput.Text = ReporterDefaultScanRoot;
        page.AddChild(_collectorScanRootInput);

        page.AddChild(CreateFieldLabel(FieldLabelKeywords));
        _collectorKeywordInput = new LineEdit();
        _collectorKeywordInput.Text = CollectorDefaultKeywords;
        page.AddChild(_collectorKeywordInput);

        var actions = new HBoxContainer();
        actions.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        page.AddChild(actions);

        var runButton = new Button();
        runButton.Text = ButtonRunCollectorPrototype;
        runButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        runButton.Pressed += RunCollectorPrototype;
        actions.AddChild(runButton);

        var loadRulesButton = new Button();
        loadRulesButton.Text = ButtonLoadCollectorRules;
        loadRulesButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        loadRulesButton.Pressed += LoadCollectorRulesFromDisk;
        actions.AddChild(loadRulesButton);

        var saveRulesButton = new Button();
        saveRulesButton.Text = ButtonSaveCollectorRules;
        saveRulesButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        saveRulesButton.Pressed += SaveCollectorRulesToDisk;
        actions.AddChild(saveRulesButton);

        var resetRulesButton = new Button();
        resetRulesButton.Text = ButtonResetCollectorRules;
        resetRulesButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        resetRulesButton.Pressed += ResetCollectorRulesToDefault;
        actions.AddChild(resetRulesButton);

        page.AddChild(CreateFieldLabel(FieldLabelCollectorRules));
        _collectorRulesJsonEdit = new TextEdit();
        _collectorRulesJsonEdit.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _collectorRulesJsonEdit.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _collectorRulesJsonEdit.CustomMinimumSize = new Vector2(UiMinWidthAuto, CollectorViewMinHeight);
        page.AddChild(_collectorRulesJsonEdit);

        _collectorPreviewView = new RichTextLabel();
        _collectorPreviewView.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _collectorPreviewView.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _collectorPreviewView.CustomMinimumSize = new Vector2(UiMinWidthAuto, CollectorViewMinHeight);
        page.AddChild(_collectorPreviewView);

        _collectorLogView = new RichTextLabel();
        _collectorLogView.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _collectorLogView.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _collectorLogView.CustomMinimumSize = new Vector2(UiMinWidthAuto, CollectorViewMinHeight);
        page.AddChild(_collectorLogView);

        LoadCollectorRulesFromDisk();
        AppendCollectorLog(CollectorReadyLog);
        return page;
    }

    private Control CreateReporterPage()
    {
        var page = new VBoxContainer();
        page.Name = ReporterModuleName;
        page.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        page.SizeFlagsVertical = Control.SizeFlags.ExpandFill;

        var titleLabel = new Label();
        titleLabel.Text = string.Format(ModuleTitleFormat, ReporterModuleName);
        page.AddChild(titleLabel);

        var subtitleLabel = new Label();
        subtitleLabel.Text = ReporterSubtitleText;
        subtitleLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        page.AddChild(subtitleLabel);

        page.AddChild(CreateFieldLabel(FieldLabelScanRoot));
        _reporterScanRootInput = new LineEdit();
        _reporterScanRootInput.Text = ReporterDefaultScanRoot;
        page.AddChild(_reporterScanRootInput);

        page.AddChild(CreateFieldLabel(FieldLabelFilterKeyword));
        _reporterKeywordInput = new LineEdit();
        _reporterKeywordInput.Text = string.Empty;
        page.AddChild(_reporterKeywordInput);

        page.AddChild(CreateFieldLabel(FieldLabelSortMode));
        _reporterSortOptions = new OptionButton();
        _reporterSortOptions.AddItem(ReporterDefaultSortMode);
        _reporterSortOptions.AddItem(ExtensionSummarySortModeAsc);
        page.AddChild(_reporterSortOptions);

        var actions = new HBoxContainer();
        actions.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        page.AddChild(actions);

        var runButton = new Button();
        runButton.Text = ButtonRunReporterPrototype;
        runButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        runButton.Pressed += RunReporterPrototype;
        actions.AddChild(runButton);

        var loadBuildReportButton = new Button();
        loadBuildReportButton.Text = ButtonLoadBuildReport;
        loadBuildReportButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        loadBuildReportButton.Pressed += LoadLatestBuildReport;
        actions.AddChild(loadBuildReportButton);

        var exportButton = new Button();
        exportButton.Text = ButtonExportSummaryPlaceholder;
        exportButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        exportButton.Pressed += ExportReporterPlaceholder;
        actions.AddChild(exportButton);

        var quickActions = new HBoxContainer();
        quickActions.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        page.AddChild(quickActions);

        var openOutputButton = new Button();
        openOutputButton.Text = ButtonOpenLastBuildOutput;
        openOutputButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        openOutputButton.Pressed += OpenLastBuildOutputDirectory;
        quickActions.AddChild(openOutputButton);

        var openManifestButton = new Button();
        openManifestButton.Text = ButtonOpenLastBuildManifest;
        openManifestButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        openManifestButton.Pressed += OpenLastBuildManifestFile;
        quickActions.AddChild(openManifestButton);

        _reporterSummaryView = new RichTextLabel();
        _reporterSummaryView.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _reporterSummaryView.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _reporterSummaryView.CustomMinimumSize = new Vector2(UiMinWidthAuto, ReporterSummaryMinHeight);
        page.AddChild(_reporterSummaryView);

        _reporterLogView = new RichTextLabel();
        _reporterLogView.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _reporterLogView.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _reporterLogView.CustomMinimumSize = new Vector2(UiMinWidthAuto, ReporterLogMinHeight);
        page.AddChild(_reporterLogView);

        AppendReporterLog(ReporterReadyLog);
        return page;
    }

    private Control CreateDebuggerPage()
    {
        var page = new VBoxContainer();
        page.Name = DebuggerModuleName;
        page.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        page.SizeFlagsVertical = Control.SizeFlags.ExpandFill;

        var titleLabel = new Label();
        titleLabel.Text = string.Format(ModuleTitleFormat, DebuggerModuleName);
        page.AddChild(titleLabel);

        var subtitleLabel = new Label();
        subtitleLabel.Text = DebuggerSubtitleText;
        subtitleLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        page.AddChild(subtitleLabel);

        _debuggerStatusLabel = new Label();
        _debuggerStatusLabel.Text = DebuggerStatusUnchecked;
        page.AddChild(_debuggerStatusLabel);

        _debuggerPathLabel = new Label();
        _debuggerPathLabel.Text = DebuggerPathUnchecked;
        _debuggerPathLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        page.AddChild(_debuggerPathLabel);

        var actions = new HBoxContainer();
        actions.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        page.AddChild(actions);

        var refreshButton = new Button();
        refreshButton.Text = ButtonRefreshRuntimeSnapshot;
        refreshButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        refreshButton.Pressed += RefreshDebuggerSnapshot;
        actions.AddChild(refreshButton);

        var checkButton = new Button();
        checkButton.Text = ButtonRunPathDiagnostics;
        checkButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        checkButton.Pressed += RunDebuggerPathCheck;
        actions.AddChild(checkButton);

        var alignmentButton = new Button();
        alignmentButton.Text = ButtonValidateDiagnosticAlignment;
        alignmentButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        alignmentButton.Pressed += RunDiagnosticFieldAlignmentCheck;
        actions.AddChild(alignmentButton);

        var runtimeCommandFields = new HBoxContainer();
        runtimeCommandFields.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        page.AddChild(runtimeCommandFields);

        var commandLabel = new Label();
        commandLabel.Text = FieldLabelRuntimeCommand;
        runtimeCommandFields.AddChild(commandLabel);

        _debuggerCommandInput = new LineEdit();
        _debuggerCommandInput.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _debuggerCommandInput.PlaceholderText = DebuggerRuntimeCommandDefault;
        _debuggerCommandInput.Text = DebuggerRuntimeCommandDefault;
        runtimeCommandFields.AddChild(_debuggerCommandInput);

        var paramLabel = new Label();
        paramLabel.Text = FieldLabelRuntimeParam;
        runtimeCommandFields.AddChild(paramLabel);

        _debuggerCommandParamInput = new LineEdit();
        _debuggerCommandParamInput.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        runtimeCommandFields.AddChild(_debuggerCommandParamInput);

        var runtimeCommandButton = new Button();
        runtimeCommandButton.Text = ButtonRunRuntimeCommand;
        runtimeCommandButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        runtimeCommandButton.Pressed += RunRuntimeDebugCommand;
        page.AddChild(runtimeCommandButton);

        _debuggerLogView = new RichTextLabel();
        _debuggerLogView.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _debuggerLogView.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _debuggerLogView.CustomMinimumSize = new Vector2(UiMinWidthAuto, DebuggerLogMinHeight);
        page.AddChild(_debuggerLogView);

        AppendDebuggerLog(DebuggerReadyLog);
        RefreshDebuggerSnapshot();
        return page;
    }

    private Control CreateModulePage(string title, string subtitle)
    {
        var page = new VBoxContainer();
        page.Name = title;
        page.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        page.SizeFlagsVertical = Control.SizeFlags.ExpandFill;

        var titleLabel = new Label();
        titleLabel.Text = string.Format(ModuleTitleFormat, title);
        page.AddChild(titleLabel);

        var subtitleLabel = new Label();
        subtitleLabel.Text = subtitle;
        subtitleLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        page.AddChild(subtitleLabel);

        var tipLabel = new Label();
        tipLabel.Text = ModulePageTipText;
        tipLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        page.AddChild(tipLabel);

        return page;
    }

    private void RunBuilderAligned()
    {
        if (_builderPackageOptions != null && string.IsNullOrWhiteSpace(_builderPackageNameInput?.Text))
        {
            OnBuilderPackageSelected(_builderPackageOptions.Selected);
        }

        SaveBuilderProfileFromControls();
        RunBuilderPrototypeLegacy();
    }

    // Legacy builder implementation is retained for rollback/reference.
    // New builder UI flow should enter from RunBuilderAligned().
    private void RunBuilderPrototypeLegacy()
    {
        var packageName = _builderPackageNameInput?.Text?.Trim() ?? string.Empty;
        var outputPath = _builderOutputPathInput?.Text?.Trim() ?? string.Empty;
        var selectedPipeline = _builderPipelineOptions == null ? string.Empty : _builderPipelineOptions.GetItemText(_builderPipelineOptions.Selected);
        var pipeline = NormalizeBuilderPipeline(selectedPipeline);
        var profile = CaptureBuilderProfileFromControls();
        profile.BuildMode = NormalizeBuilderBuildMode(profile.BuildMode, pipeline);
        var collectorScanRoot = _collectorScanRootInput?.Text?.Trim();
        if (string.IsNullOrEmpty(collectorScanRoot))
        {
            collectorScanRoot = BuilderDefaultScanRoot;
        }

        var collectorKeywords = _collectorKeywordInput?.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(collectorKeywords))
        {
            collectorKeywords = GetDefaultPipelineKeywords(pipeline);
        }

        if (string.IsNullOrEmpty(packageName))
        {
            SetStatus(BuilderParamErrorPackageNameStatus);
            AppendBuilderLog(BuilderParamErrorPackageNameLog);
            return;
        }

        if (string.IsNullOrEmpty(outputPath))
        {
            SetStatus(BuilderParamErrorOutputPathStatus);
            AppendBuilderLog(BuilderParamErrorOutputPathLog);
            return;
        }

        string globalOutputPath;
        try
        {
            globalOutputPath = _editorPlatformBridge.GlobalizePath(outputPath);
            Directory.CreateDirectory(globalOutputPath);
        }
        catch (Exception ex)
        {
            SetStatus(BuilderRunStatusOutputPathUnavailable);
            AppendBuilderLog($"{BuilderRunLogFailedPrefix}{ex.Message}");
            return;
        }

        AppendBuilderLog(BuilderRunLogStart);
        AppendBuilderLog($"{BuilderRunLogPackageNamePrefix}{packageName}");
        AppendBuilderLog($"{BuilderRunLogOutputPathPrefix}{outputPath}");
        AppendBuilderLog($"{BuilderRunLogGlobalOutputPrefix}{globalOutputPath}");
        AppendBuilderLog($"{BuilderRunLogPipelinePrefix}{pipeline}");
        AppendBuilderLog($"{BuilderRunLogPipelineProfilePrefix}{GetBuilderPipelineProfile(pipeline).Description}");
        AppendBuilderLog($"{BuilderRunLogScanRootPrefix}{collectorScanRoot}");
        AppendBuilderLog($"{BuilderRunLogKeywordsPrefix}{collectorKeywords}");
        AppendBuilderLog($"{BuilderRunLogBuildModePrefix}{profile.BuildMode}");
        AppendBuilderLog($"{BuilderRunLogEncryptionPrefix}{profile.Encryption}");
        AppendBuilderLog($"{BuilderRunLogCompressionPrefix}{profile.Compression}");
        AppendBuilderLog($"{BuilderRunLogFileNameStylePrefix}{profile.FileNameStyle}");
        AppendBuilderLog($"{BuilderRunLogCopyBuildinOptionPrefix}{profile.CopyBuildinFileOption}");
        AppendBuilderLog($"{BuilderRunLogCopyBuildinParamPrefix}{profile.CopyBuildinFileParam}");

        try
        {
            var buildResult = ExecuteBuilderBuild(packageName, globalOutputPath, pipeline, collectorScanRoot, collectorKeywords, profile);
            _lastBuildPackageName = buildResult.PackageName;
            _lastBuildPipeline = buildResult.Pipeline;
            _lastBuildOutputDirectory = buildResult.OutputDirectory;
            _lastBuildManifestPath = buildResult.ManifestPath;
            _lastBuildFileCount = buildResult.FileCount;
            _lastBuildScannedCount = buildResult.ScannedCount;
            _lastBuildScanLimitHit = buildResult.ScanLimitHit;
            _lastBuildTime = buildResult.BuildTime;
            SyncReporterInputsFromBuilder(collectorScanRoot, collectorKeywords);
            AppendBuilderLog(BuilderRunLogCompleted);
            AppendBuilderLog(buildResult.Summary);
            SetStatus($"{BuilderRunStatusCompletedPrefix}{packageName}");
        }
        catch (Exception ex)
        {
            AppendBuilderLog($"{BuilderRunLogBuildFailedPrefix}{ex.Message}");
            SetStatus(BuilderRunStatusFailed);
        }
    }

    private void AppendBuilderLog(string message)
    {
        AppendLogEntry(_builderLogView, message);
    }

    private void SyncReporterInputsFromBuilder(string scanRoot, string keywordsText)
    {
        if (_reporterScanRootInput != null)
        {
            _reporterScanRootInput.Text = scanRoot;
        }

        if (_reporterKeywordInput != null)
        {
            _reporterKeywordInput.Text = keywordsText;
        }

        AppendBuilderLog($"{BuilderSyncReporterLogPrefix}{scanRoot}{BuilderSyncReporterLogMid}{keywordsText}");
        AppendReporterLog(BuilderSyncReporterToReporterLog);
    }

    private BuildExecutionResult ExecuteBuilderBuild(string packageName, string globalOutputPath, string pipeline, string scanRoot, string keywordsText, BuilderProfileSettings profile)
    {
        var context = new BuildOrchestrationContext
        {
            PackageName = packageName,
            GlobalOutputPath = globalOutputPath,
            Pipeline = pipeline,
            ScanRoot = scanRoot,
            KeywordsText = keywordsText,
            BuildMode = profile.BuildMode,
            Encryption = profile.Encryption,
            Compression = profile.Compression,
            FileNameStyle = profile.FileNameStyle,
            CopyBuildinFileOption = profile.CopyBuildinFileOption,
            CopyBuildinFileParam = profile.CopyBuildinFileParam,
            RequestedPackageVersion = profile.PackageVersion
        };

        RunPrepareBuildStage(context);
        RunPackageBuildStage(context);
        RunManifestBuildStage(context);
        RunRuntimeLinkBuildStage(context);
        RunCopyBuildinStage(context);
        RunVerifyBuildStage(context);
        return new BuildExecutionResult
        {
            PackageName = context.PackageName,
            Pipeline = context.Pipeline,
            PackageVersion = context.PackageVersion,
            OutputDirectory = context.PackageRoot,
            ManifestPath = context.ManifestPath,
            FileCount = context.CopiedCount,
            ScannedCount = context.ScannedCount,
            ScanLimitHit = context.ScanLimitHit,
            BuildTime = context.BuildTime,
            Summary = string.Format(BuilderRunSummaryFormat, context.Pipeline, context.PackageVersion, context.ScannedCount, context.CopiedCount, context.ScanLimitHit, context.PackageRoot)
        };
    }

    private void RunPrepareBuildStage(BuildOrchestrationContext context)
    {
        AppendBuilderLog($"{BuilderStageStartPrefix}{BuilderStagePrepare}");
        if (!TryCollectFiles(context.ScanRoot, context.KeywordsText, context.Pipeline, BuilderScanLimit, out var sourceRoot, out var collectedFiles, out var scannedCount, out var scanLimitHit, out var errorMessage))
        {
            throw new InvalidOperationException(errorMessage);
        }

        if (collectedFiles.Count == 0)
        {
            throw new InvalidOperationException(BuilderNoBuildResourceError);
        }

        context.SourceRoot = sourceRoot;
        context.CollectedFiles = collectedFiles;
        context.ScannedCount = scannedCount;
        context.ScanLimitHit = scanLimitHit;
        context.BuildTime = DateTime.Now;
        context.BuildTimestamp = context.BuildTime.ToString(BuilderBuildTimestampFormat);
        context.PackageVersion = string.IsNullOrWhiteSpace(context.RequestedPackageVersion)
            ? context.BuildTime.ToString(BuilderPackageVersionFormat)
            : context.RequestedPackageVersion;
        var pipelineProfile = GetBuilderPipelineProfile(context.Pipeline);
        context.RuntimeOutputNameStyle = ResolveRuntimeOutputNameStyle(context.FileNameStyle, context.Pipeline);
        if (pipelineProfile.RuntimeOutputNameStyleOverride >= 0)
        {
            AppendBuilderLog($"{BuilderRunLogFileNameStyleForcedPrefix}{context.Pipeline} => OutputNameStyle={context.RuntimeOutputNameStyle}");
        }

        context.SkipWriteOutputs = IsBuilderNoOutputMode(context.BuildMode);
        context.EncryptionServices = CreateBuilderEncryptionInstance(context.Encryption);
        context.BuildinRoot = ResolveBuildinPackageRoot(context.PackageName);
        context.PackageRoot = Path.Combine(context.GlobalOutputPath, context.PackageName, context.PackageVersion);
        if (context.RuntimeOutputNameStyle == 1)
        {
            context.FilesRoot = Path.Combine(context.PackageRoot, BuilderFilesDirectoryName);
        }
        else
        {
            context.FilesRoot = context.PackageRoot;
        }

        if (context.SkipWriteOutputs)
        {
            AppendBuilderLog($"{BuilderModeNoOutputLog}{context.BuildMode}");
            AppendBuilderLog(BuilderCopyBuildinSkippedLog);
            AppendBuilderLog(BuilderCompressionIgnoredLog);
        }
        else
        {
            if (IsBuilderForceRebuildMode(context.BuildMode) && Directory.Exists(context.PackageRoot))
            {
                Directory.Delete(context.PackageRoot, true);
            }

            if (Directory.Exists(context.PackageRoot) && !IsBuilderIncrementalMode(context.BuildMode))
            {
                throw new InvalidOperationException(BuilderVerifyErrorOutputAlreadyExists);
            }

            Directory.CreateDirectory(context.PackageRoot);
            Directory.CreateDirectory(context.FilesRoot);
            AppendBuilderLog(BuilderCompressionIgnoredLog);
        }

        AppendBuilderLog($"{BuilderStageCompletedPrefix}{BuilderStagePrepare}");
    }

    private void RunPackageBuildStage(BuildOrchestrationContext context)
    {
        AppendBuilderLog($"{BuilderStageStartPrefix}{BuilderStagePackage}");
        context.CopiedCount = 0;
        context.BundleEntries.Clear();
        foreach (var sourceFile in context.CollectedFiles)
        {
            var relativePath = Path.GetRelativePath(context.SourceRoot, sourceFile);
            var bundleName = NormalizePathForDisplay(Path.Combine(BuilderFilesDirectoryName, relativePath));
            var sourceBytes = File.ReadAllBytes(sourceFile);
            var (outputBytes, encrypted) = EncryptBuilderFileData(context, bundleName, sourceFile, sourceBytes);
            var fileHash = HashUtility.BytesMD5(outputBytes);
            var fileCrc = HashUtility.BytesCRC32(outputBytes);
            var fileExtension = ManifestTools.GetRemoteBundleFileExtension(bundleName);
            var outputFileName = ManifestTools.GetRemoteBundleFileName(context.RuntimeOutputNameStyle, bundleName, fileExtension, fileHash);
            var destinationPath = Path.Combine(context.PackageRoot, outputFileName);
            if (!context.SkipWriteOutputs)
            {
                WriteBuilderOutputFile(context, destinationPath, outputBytes);
            }

            context.BundleEntries.Add(new BuilderBundleEntry
            {
                SourceFilePath = sourceFile,
                BundleName = bundleName,
                OutputFileName = outputFileName,
                DestinationFilePath = destinationPath,
                FileHash = fileHash,
                FileCRC = fileCrc,
                FileSize = outputBytes.LongLength,
                Encrypted = encrypted,
                Tags = InferBuilderTags(relativePath, sourceFile)
            });
            context.CopiedCount++;
        }

        AppendBuilderLog($"{BuilderStageCompletedPrefix}{BuilderStagePackage}");
    }

    private void RunManifestBuildStage(BuildOrchestrationContext context)
    {
        AppendBuilderLog($"{BuilderStageStartPrefix}{BuilderStageManifest}");
        context.ManifestPath = Path.Combine(context.PackageRoot, BuilderManifestFileName);
        context.VersionFilePath = Path.Combine(context.PackageRoot, BuilderVersionFileName);
        if (!context.SkipWriteOutputs)
        {
            File.WriteAllText(context.ManifestPath, BuildManifestText(context), Encoding.UTF8);
            File.WriteAllText(context.VersionFilePath, BuildVersionText(context), Encoding.UTF8);
        }

        AppendBuilderLog($"{BuilderStageCompletedPrefix}{BuilderStageManifest}");
    }

    private void RunRuntimeLinkBuildStage(BuildOrchestrationContext context)
    {
        AppendBuilderLog($"{BuilderStageStartPrefix}{BuilderStageRuntimeLink}");
        context.RuntimeVersionFilePath = Path.Combine(context.PackageRoot, YooAssetSettingsData.GetPackageVersionFileName(context.PackageName));
        context.RuntimeManifestFilePath = Path.Combine(context.PackageRoot, YooAssetSettingsData.GetManifestBinaryFileName(context.PackageName, context.PackageVersion));
        context.RuntimeHashFilePath = Path.Combine(context.PackageRoot, YooAssetSettingsData.GetPackageHashFileName(context.PackageName, context.PackageVersion));
        if (context.SkipWriteOutputs)
        {
            AppendBuilderLog($"{BuilderStageCompletedPrefix}{BuilderStageRuntimeLink}");
            return;
        }

        var outputNameStyle = context.RuntimeOutputNameStyle;
        if (outputNameStyle != 1)
        {
            AppendBuilderLog($"{BuilderRunLogFileNameStylePrefix}{context.FileNameStyle}");
        }

        var runtimeManifest = new PackageManifest
        {
            FileVersion = YooAssetSettings.ManifestFileVersion,
            EnableAddressable = false,
            LocationToLower = false,
            IncludeAssetGUID = false,
            OutputNameStyle = outputNameStyle,
            BuildPipeline = context.Pipeline,
            PackageName = context.PackageName,
            PackageVersion = context.PackageVersion,
            AssetList = new List<PackageAsset>(context.BundleEntries.Count),
            BundleList = new List<PackageBundle>(context.BundleEntries.Count)
        };

        for (var index = 0; index < context.BundleEntries.Count; index++)
        {
            var entry = context.BundleEntries[index];
            runtimeManifest.BundleList.Add(new PackageBundle
            {
                BundleName = entry.BundleName,
                UnityCRC = 0,
                FileHash = entry.FileHash,
                FileCRC = entry.FileCRC,
                FileSize = entry.FileSize,
                Encrypted = entry.Encrypted,
                Tags = entry.Tags,
                DependIDs = Array.Empty<int>()
            });

            var assetPath = BuildRuntimeManifestAssetPath(entry.SourceFilePath);
            runtimeManifest.AssetList.Add(new PackageAsset
            {
                Address = assetPath,
                AssetPath = assetPath,
                AssetGUID = string.Empty,
                AssetTags = Array.Empty<string>(),
                BundleID = index
            });
        }

        SerializeRuntimeManifestBinary(context.RuntimeManifestFilePath, runtimeManifest);
        var runtimeManifestHash = HashUtility.FileMD5(context.RuntimeManifestFilePath);
        File.WriteAllText(context.RuntimeVersionFilePath, context.PackageVersion, Encoding.UTF8);
        File.WriteAllText(context.RuntimeHashFilePath, runtimeManifestHash, Encoding.UTF8);
        AppendBuilderLog($"{BuilderStageCompletedPrefix}{BuilderStageRuntimeLink}");
    }

    private void RunCopyBuildinStage(BuildOrchestrationContext context)
    {
        AppendBuilderLog($"{BuilderStageStartPrefix}{BuilderStageCopyBuildin}");
        if (context.SkipWriteOutputs)
        {
            AppendBuilderLog(BuilderCopyBuildinSkippedLog);
            AppendBuilderLog($"{BuilderStageCompletedPrefix}{BuilderStageCopyBuildin}");
            return;
        }

        if (string.Equals(context.CopyBuildinFileOption, BuilderCopyBuildinFileOptionNone, StringComparison.OrdinalIgnoreCase))
        {
            AppendBuilderLog(BuilderCopyBuildinDisabledLog);
            AppendBuilderLog($"{BuilderStageCompletedPrefix}{BuilderStageCopyBuildin}");
            return;
        }

        var copyAll = string.Equals(context.CopyBuildinFileOption, BuilderCopyBuildinFileOptionClearAndCopyAll, StringComparison.OrdinalIgnoreCase)
                      || string.Equals(context.CopyBuildinFileOption, BuilderCopyBuildinFileOptionOnlyCopyAll, StringComparison.OrdinalIgnoreCase);
        var clearFirst = string.Equals(context.CopyBuildinFileOption, BuilderCopyBuildinFileOptionClearAndCopyAll, StringComparison.OrdinalIgnoreCase)
                         || string.Equals(context.CopyBuildinFileOption, BuilderCopyBuildinFileOptionClearAndCopyByTags, StringComparison.OrdinalIgnoreCase);
        var targets = copyAll
            ? context.BundleEntries
            : FilterBuildinCopyTargets(context.BundleEntries, context.CopyBuildinFileParam);
        if (clearFirst && Directory.Exists(context.BuildinRoot))
        {
            Directory.Delete(context.BuildinRoot, true);
        }

        Directory.CreateDirectory(context.BuildinRoot);
        AppendBuilderLog($"{BuilderCopyBuildinRootPrefix}{context.BuildinRoot}");
        CopyFileToBuildinRoot(context.RuntimeVersionFilePath, context.BuildinRoot);
        CopyFileToBuildinRoot(context.RuntimeManifestFilePath, context.BuildinRoot);
        CopyFileToBuildinRoot(context.RuntimeHashFilePath, context.BuildinRoot);
        if (!copyAll && targets.Count == 0)
        {
            AppendBuilderLog(BuilderCopyBuildinNoMatchLog);
            AppendBuilderLog($"{BuilderStageCompletedPrefix}{BuilderStageCopyBuildin}");
            return;
        }

        var copiedCount = 0;
        for (var index = 0; index < targets.Count; index++)
        {
            var entry = targets[index];
            var destinationPath = Path.Combine(context.BuildinRoot, entry.OutputFileName);
            var destinationDir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            File.Copy(entry.DestinationFilePath, destinationPath, true);
            copiedCount++;
        }

        AppendBuilderLog($"{BuilderCopyBuildinCopiedPrefix}{copiedCount}");
        AppendBuilderLog($"{BuilderStageCompletedPrefix}{BuilderStageCopyBuildin}");
    }

    private void RunVerifyBuildStage(BuildOrchestrationContext context)
    {
        AppendBuilderLog($"{BuilderStageStartPrefix}{BuilderStageVerify}");
        if (context.SkipWriteOutputs)
        {
            AppendBuilderLog($"{BuilderStageCompletedPrefix}{BuilderStageVerify}");
            return;
        }

        if (!Directory.Exists(context.FilesRoot))
        {
            throw new InvalidOperationException(BuilderVerifyErrorOutputRootMissing);
        }

        if (!File.Exists(context.ManifestPath))
        {
            throw new InvalidOperationException(BuilderVerifyErrorManifestMissing);
        }

        if (!File.Exists(context.VersionFilePath))
        {
            throw new InvalidOperationException(BuilderVerifyErrorVersionFileMissing);
        }

        if (!File.Exists(context.RuntimeVersionFilePath))
        {
            throw new InvalidOperationException(BuilderVerifyErrorRuntimeVersionFileMissing);
        }

        if (!File.Exists(context.RuntimeManifestFilePath))
        {
            throw new InvalidOperationException(BuilderVerifyErrorRuntimeManifestFileMissing);
        }

        if (!File.Exists(context.RuntimeHashFilePath))
        {
            throw new InvalidOperationException(BuilderVerifyErrorRuntimeHashFileMissing);
        }

        if (context.CopiedCount != context.CollectedFiles.Count)
        {
            throw new InvalidOperationException(BuilderVerifyErrorFileCountMismatch);
        }

        foreach (var entry in context.BundleEntries)
        {
            if (!File.Exists(entry.DestinationFilePath))
            {
                throw new InvalidOperationException($"{BuilderVerifyErrorFileMissingPrefix}{entry.DestinationFilePath}");
            }
        }

        AppendBuilderLog($"{BuilderStageCompletedPrefix}{BuilderStageVerify}");
    }

    private static bool IsBuilderSourceFileIncluded(string absolutePath, string pipeline)
    {
        var profile = GetBuilderPipelineProfile(pipeline);
        var normalized = NormalizePathForDisplay(absolutePath);
        if (normalized.Contains(SourceFilterIgnoreDirectory, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (normalized.EndsWith(SourceFilterIgnoreImportExtension, StringComparison.OrdinalIgnoreCase) ||
            normalized.EndsWith(SourceFilterIgnoreUidExtension, StringComparison.OrdinalIgnoreCase) ||
            normalized.EndsWith(SourceFilterIgnoreScriptExtension, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var extension = Path.GetExtension(normalized);
        if (string.IsNullOrEmpty(extension))
        {
            return false;
        }

        if (profile.SupportsRawFiles)
        {
            return RawFilePipelineExtensions.Contains(extension);
        }

        return ScenePipelineExtensions.Contains(extension);
    }

    private static string NormalizeBuilderPipeline(string pipeline)
    {
        if (string.IsNullOrWhiteSpace(pipeline))
        {
            return ReporterDefaultPipeline;
        }

        if (string.Equals(pipeline, BuilderPipelineGodotDisplay, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(pipeline, BuilderPipelineGodotDisplayLegacy, StringComparison.OrdinalIgnoreCase))
        {
            return ReporterDefaultPipeline;
        }

        if (string.Equals(pipeline, BuilderPipelineBuiltinDisplay, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(pipeline, BuilderPipelineBuiltinDisplayLegacy, StringComparison.OrdinalIgnoreCase))
        {
            return BuilderPipelineBuiltin;
        }

        if (string.Equals(pipeline, BuilderPipelineScriptableDisplay, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(pipeline, BuilderPipelineScriptableDisplayLegacy, StringComparison.OrdinalIgnoreCase))
        {
            return BuilderPipelineScriptable;
        }

        if (string.Equals(pipeline, ReporterDefaultPipeline, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(pipeline, BuilderPipelineBuiltin, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(pipeline, BuilderPipelineScriptable, StringComparison.OrdinalIgnoreCase))
        {
            return pipeline;
        }

        return ReporterDefaultPipeline;
    }

    private static string NormalizeBuilderBuildMode(string buildMode, string pipeline)
    {
        var supportedModes = GetSupportedBuilderModes(pipeline);
        if (supportedModes.Count == 0)
        {
            return BuilderDefaultBuildMode;
        }

        for (var index = 0; index < supportedModes.Count; index++)
        {
            if (string.Equals(supportedModes[index], buildMode, StringComparison.OrdinalIgnoreCase))
            {
                return supportedModes[index];
            }
        }

        return supportedModes[0];
    }

    private static int ResolveRuntimeOutputNameStyle(string fileNameStyle, string pipeline)
    {
        var profile = GetBuilderPipelineProfile(pipeline);
        if (profile.RuntimeOutputNameStyleOverride >= 0)
        {
            return profile.RuntimeOutputNameStyleOverride;
        }

        if (string.Equals(fileNameStyle, BuilderFileNameStyleHashName, StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        if (string.Equals(fileNameStyle, BuilderFileNameStyleBundleNameHashName, StringComparison.OrdinalIgnoreCase))
        {
            return 2;
        }

        return 1;
    }

    private static bool IsBuilderNoOutputMode(string buildMode)
    {
        return string.Equals(buildMode, BuilderBuildModeDryRunBuild, StringComparison.OrdinalIgnoreCase)
               || string.Equals(buildMode, BuilderBuildModeSimulateBuild, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBuilderForceRebuildMode(string buildMode)
    {
        return string.Equals(buildMode, BuilderBuildModeForceRebuild, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBuilderIncrementalMode(string buildMode)
    {
        return string.Equals(buildMode, BuilderBuildModeIncrementalBuild, StringComparison.OrdinalIgnoreCase);
    }

    private IEncryptionServices CreateBuilderEncryptionInstance(string encryptionTypeName)
    {
        if (string.IsNullOrWhiteSpace(encryptionTypeName) || string.Equals(encryptionTypeName, BuilderDefaultEncryption, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var encryptionTypes = FindEncryptionImplementations();
        for (var index = 0; index < encryptionTypes.Count; index++)
        {
            var type = encryptionTypes[index];
            if (!string.Equals(type.FullName, encryptionTypeName, StringComparison.Ordinal))
            {
                continue;
            }

            if (Activator.CreateInstance(type) is IEncryptionServices services)
            {
                return services;
            }
        }

        throw new InvalidOperationException($"Encryption type not found: {encryptionTypeName}");
    }

    private static (byte[] OutputBytes, bool Encrypted) EncryptBuilderFileData(BuildOrchestrationContext context, string bundleName, string sourceFilePath, byte[] sourceBytes)
    {
        if (context.EncryptionServices == null)
        {
            return (sourceBytes, false);
        }

        var fileInfo = new EncryptFileInfo
        {
            BundleName = bundleName,
            FileLoadPath = sourceFilePath
        };
        var result = context.EncryptionServices.Encrypt(fileInfo);
        if (!result.Encrypted)
        {
            return (sourceBytes, false);
        }

        if (result.EncryptedData == null || result.EncryptedData.Length == 0)
        {
            throw new InvalidOperationException($"Encryption output invalid: {bundleName}");
        }

        return (result.EncryptedData, true);
    }

    private static void WriteBuilderOutputFile(BuildOrchestrationContext context, string destinationPath, byte[] outputBytes)
    {
        var destinationDir = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(destinationDir))
        {
            Directory.CreateDirectory(destinationDir);
        }

        if (IsBuilderIncrementalMode(context.BuildMode) && File.Exists(destinationPath))
        {
            var existsHash = HashUtility.FileMD5(destinationPath);
            var targetHash = HashUtility.BytesMD5(outputBytes);
            if (string.Equals(existsHash, targetHash, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        File.WriteAllBytes(destinationPath, outputBytes);
    }

    private static string[] InferBuilderTags(string relativePath, string sourceFilePath)
    {
        var tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var extension = Path.GetExtension(sourceFilePath)?.TrimStart('.');
        if (!string.IsNullOrEmpty(extension))
        {
            tags.Add(extension.ToLowerInvariant());
        }

        var normalizedRelative = NormalizePathForDisplay(relativePath);
        var segments = normalizedRelative.Split('/', StringSplitOptions.RemoveEmptyEntries);
        for (var index = 0; index < segments.Length - 1; index++)
        {
            tags.Add(segments[index].ToLowerInvariant());
        }

        return tags.ToArray();
    }

    private static string ResolveBuildinPackageRoot(string packageName)
    {
        var defaultFolderName = YooAssetSettingsData.Setting.DefaultYooFolderName;
        var rootPath = Path.Combine(UnityEngine.Application.streamingAssetsPath, defaultFolderName, packageName);
        return NormalizePathForDisplay(rootPath);
    }

    private static List<BuilderBundleEntry> FilterBuildinCopyTargets(List<BuilderBundleEntry> entries, string copyParams)
    {
        var tokens = copyParams
            .Split(new[] { ';', ',', '|', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (tokens.Length == 0)
        {
            return new List<BuilderBundleEntry>();
        }

        var result = new List<BuilderBundleEntry>();
        for (var index = 0; index < entries.Count; index++)
        {
            var entry = entries[index];
            if (IsBuilderBundleTagMatched(entry, tokens))
            {
                result.Add(entry);
            }
        }

        return result;
    }

    private static bool IsBuilderBundleTagMatched(BuilderBundleEntry entry, string[] tokens)
    {
        if (entry.Tags == null || entry.Tags.Length == 0)
        {
            return false;
        }

        for (var tokenIndex = 0; tokenIndex < tokens.Length; tokenIndex++)
        {
            var token = tokens[tokenIndex];
            if (string.IsNullOrWhiteSpace(token))
            {
                continue;
            }

            var normalizedToken = token.Trim().TrimStart('.').ToLowerInvariant();
            for (var tagIndex = 0; tagIndex < entry.Tags.Length; tagIndex++)
            {
                if (string.Equals(entry.Tags[tagIndex], normalizedToken, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            if (entry.BundleName.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static void CopyFileToBuildinRoot(string sourcePath, string buildinRoot)
    {
        if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath))
        {
            return;
        }

        var fileName = Path.GetFileName(sourcePath);
        var destinationPath = Path.Combine(buildinRoot, fileName);
        File.Copy(sourcePath, destinationPath, true);
    }

    private static string GetDefaultPipelineKeywords(string pipeline)
    {
        return GetBuilderPipelineProfile(pipeline).DefaultKeywords;
    }

    private static CollectorRuleModel ParseCollectorRuleModel(string keywordsText)
    {
        var model = new CollectorRuleModel();
        if (string.IsNullOrWhiteSpace(keywordsText))
        {
            return model;
        }

        var tokens = keywordsText.Split(KeywordSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        foreach (var token in tokens)
        {
            if (token.StartsWith("include:", StringComparison.OrdinalIgnoreCase))
            {
                model.IncludeKeywords.Add(token.Substring("include:".Length));
                continue;
            }

            if (token.StartsWith("exclude:", StringComparison.OrdinalIgnoreCase))
            {
                model.ExcludeKeywords.Add(token.Substring("exclude:".Length));
                continue;
            }

            if (token.StartsWith("ext:", StringComparison.OrdinalIgnoreCase))
            {
                var extension = NormalizeCollectorExtension(token.Substring("ext:".Length));
                if (!string.IsNullOrEmpty(extension))
                {
                    model.IncludeExtensions.Add(extension);
                }

                continue;
            }

            if (token.StartsWith("exclude_ext:", StringComparison.OrdinalIgnoreCase))
            {
                var extension = NormalizeCollectorExtension(token.Substring("exclude_ext:".Length));
                if (!string.IsNullOrEmpty(extension))
                {
                    model.ExcludeExtensions.Add(extension);
                }

                continue;
            }

            if (token.StartsWith("!", StringComparison.OrdinalIgnoreCase))
            {
                var excluded = token.Substring(1);
                if (!string.IsNullOrEmpty(excluded))
                {
                    model.ExcludeKeywords.Add(excluded);
                }

                continue;
            }

            if (token.StartsWith(".", StringComparison.OrdinalIgnoreCase))
            {
                var extension = NormalizeCollectorExtension(token);
                if (!string.IsNullOrEmpty(extension))
                {
                    model.IncludeExtensions.Add(extension);
                }

                continue;
            }

            model.IncludeKeywords.Add(token);
        }

        return model;
    }

    private static string NormalizeCollectorExtension(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim();
        if (!normalized.StartsWith(".", StringComparison.Ordinal))
        {
            normalized = $".{normalized}";
        }

        return normalized.ToLowerInvariant();
    }

    private static bool IsCollectorRuleMatch(string filePath, CollectorRuleModel model)
    {
        var normalizedPath = NormalizePathForDisplay(filePath);
        var extension = Path.GetExtension(normalizedPath).ToLowerInvariant();

        if (model.IncludeExtensions.Count > 0 && !model.IncludeExtensions.Contains(extension))
        {
            return false;
        }

        if (model.ExcludeExtensions.Contains(extension))
        {
            return false;
        }

        if (model.IncludeKeywords.Count > 0)
        {
            var matchedInclude = false;
            foreach (var keyword in model.IncludeKeywords)
            {
                if (normalizedPath.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    matchedInclude = true;
                    break;
                }
            }

            if (!matchedInclude)
            {
                return false;
            }
        }

        foreach (var keyword in model.ExcludeKeywords)
        {
            if (normalizedPath.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private bool TryCollectFiles(string scanRoot, string keywordsText, string pipeline, int scanLimit, out string globalScanRoot, out List<string> matchedFiles, out int scannedCount, out bool scanLimitHit, out string errorMessage)
    {
        var collectorRuleModel = ParseCollectorRuleModel(keywordsText);
        return TryCollectFiles(scanRoot, collectorRuleModel, pipeline, scanLimit, out globalScanRoot, out matchedFiles, out scannedCount, out scanLimitHit, out errorMessage);
    }

    private bool TryCollectFiles(string scanRoot, CollectorRuleModel collectorRuleModel, string pipeline, int scanLimit, out string globalScanRoot, out List<string> matchedFiles, out int scannedCount, out bool scanLimitHit, out string errorMessage)
    {
        matchedFiles = new List<string>();
        scannedCount = 0;
        scanLimitHit = false;
        errorMessage = string.Empty;
        globalScanRoot = string.Empty;

        if (string.IsNullOrEmpty(scanRoot))
        {
            errorMessage = ScanRootEmptyError;
            return false;
        }

        globalScanRoot = _editorPlatformBridge.GlobalizePath(scanRoot);
        if (!Directory.Exists(globalScanRoot))
        {
            errorMessage = $"{ScanRootNotExistsErrorPrefix}{globalScanRoot}";
            return false;
        }

        foreach (var file in Directory.EnumerateFiles(globalScanRoot, FileSearchPatternAll, SearchOption.AllDirectories))
        {
            scannedCount++;
            if (scannedCount > scanLimit)
            {
                scanLimitHit = true;
                break;
            }

            if (!IsBuilderSourceFileIncluded(file, pipeline))
            {
                continue;
            }

            if (!IsCollectorRuleMatch(file, collectorRuleModel))
            {
                continue;
            }

            matchedFiles.Add(file);
        }

        return true;
    }

    private string BuildManifestText(BuildOrchestrationContext context)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"{ManifestFieldPackageName}={context.PackageName}");
        builder.AppendLine($"{ManifestFieldPipeline}={context.Pipeline}");
        builder.AppendLine($"{ManifestFieldBuildVersion}={context.PackageVersion}");
        builder.AppendLine($"{ManifestFieldBuildTime}={context.BuildTime.ToString(ExportIsoDateTimeFormat)}");
        builder.AppendLine($"{ManifestFieldBuildMode}={context.BuildMode}");
        builder.AppendLine($"{ManifestFieldEncryption}={context.Encryption}");
        builder.AppendLine($"{ManifestFieldCompression}={context.Compression}");
        builder.AppendLine($"{ManifestFieldFileNameStyle}={context.FileNameStyle}");
        builder.AppendLine($"{ManifestFieldCopyBuildinFileOption}={context.CopyBuildinFileOption}");
        builder.AppendLine($"{ManifestFieldCopyBuildinFileParam}={context.CopyBuildinFileParam}");
        builder.AppendLine($"{ManifestFieldSourceRoot}={NormalizePathForDisplay(context.SourceRoot)}");
        builder.AppendLine($"{ManifestFieldOutputFilesRoot}={NormalizePathForDisplay(context.FilesRoot)}");
        builder.AppendLine($"{ManifestFieldFileCount}={context.CollectedFiles.Count}");
        builder.AppendLine(ManifestSectionFiles);

        foreach (var file in context.CollectedFiles)
        {
            builder.AppendLine(Path.GetFileName(file));
            builder.AppendLine(ToProjectDisplayPath(file));
        }

        return builder.ToString();
    }

    private string BuildVersionText(BuildOrchestrationContext context)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"{ManifestFieldPackageName}={context.PackageName}");
        builder.AppendLine($"{ManifestFieldPipeline}={context.Pipeline}");
        builder.AppendLine($"{ManifestFieldBuildVersion}={context.PackageVersion}");
        builder.AppendLine($"{ManifestFieldBuildTime}={context.BuildTime.ToString(ExportIsoDateTimeFormat)}");
        builder.AppendLine($"{ManifestFieldBuildMode}={context.BuildMode}");
        builder.AppendLine($"{ManifestFieldEncryption}={context.Encryption}");
        builder.AppendLine($"{ManifestFieldCompression}={context.Compression}");
        builder.AppendLine($"{ManifestFieldFileNameStyle}={context.FileNameStyle}");
        builder.AppendLine($"{ManifestFieldCopyBuildinFileOption}={context.CopyBuildinFileOption}");
        builder.AppendLine($"{ManifestFieldCopyBuildinFileParam}={context.CopyBuildinFileParam}");
        builder.AppendLine($"{ManifestFieldOutputFilesRoot}={NormalizePathForDisplay(context.FilesRoot)}");
        builder.AppendLine($"{ManifestFieldFileCount}={context.CopiedCount}");
        builder.AppendLine($"{BuilderManifestFileName}={Path.GetFileName(context.ManifestPath)}");
        return builder.ToString();
    }

    private string BuildRuntimeManifestAssetPath(string sourceFile)
    {
        var sourceAssetPath = ToProjectDisplayPath(sourceFile);
        if (string.IsNullOrEmpty(sourceAssetPath))
        {
            return NormalizePathForDisplay(sourceFile);
        }

        return sourceAssetPath;
    }

    private void SerializeRuntimeManifestBinary(string savePath, PackageManifest manifest)
    {
        using var fileStream = new FileStream(savePath, System.IO.FileMode.Create, System.IO.FileAccess.Write);
        using var writer = new BinaryWriter(fileStream, Encoding.UTF8, false);
        writer.Write(YooAssetSettings.ManifestFileSign);
        WriteUtf8(writer, manifest.FileVersion);
        writer.Write(manifest.EnableAddressable);
        writer.Write(manifest.LocationToLower);
        writer.Write(manifest.IncludeAssetGUID);
        writer.Write(manifest.OutputNameStyle);
        WriteUtf8(writer, manifest.BuildPipeline);
        WriteUtf8(writer, manifest.PackageName);
        WriteUtf8(writer, manifest.PackageVersion);
        writer.Write(manifest.AssetList.Count);
        foreach (var packageAsset in manifest.AssetList)
        {
            WriteUtf8(writer, packageAsset.Address);
            WriteUtf8(writer, packageAsset.AssetPath);
            WriteUtf8(writer, packageAsset.AssetGUID);
            WriteUtf8Array(writer, packageAsset.AssetTags);
            writer.Write(packageAsset.BundleID);
        }

        writer.Write(manifest.BundleList.Count);
        foreach (var packageBundle in manifest.BundleList)
        {
            WriteUtf8(writer, packageBundle.BundleName);
            writer.Write(packageBundle.UnityCRC);
            WriteUtf8(writer, packageBundle.FileHash);
            WriteUtf8(writer, packageBundle.FileCRC);
            writer.Write(packageBundle.FileSize);
            writer.Write(packageBundle.Encrypted);
            WriteUtf8Array(writer, packageBundle.Tags);
            WriteInt32Array(writer, packageBundle.DependIDs);
        }

        writer.Flush();
    }

    private void WriteUtf8(BinaryWriter writer, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            writer.Write((ushort)0);
            return;
        }

        var utf8Bytes = Encoding.UTF8.GetBytes(value);
        if (utf8Bytes.Length > ushort.MaxValue)
        {
            throw new InvalidOperationException("Runtime 清单字段长度超过 UInt16 上限。");
        }

        writer.Write((ushort)utf8Bytes.Length);
        writer.Write(utf8Bytes);
    }

    private void WriteUtf8Array(BinaryWriter writer, string[] values)
    {
        if (values == null || values.Length == 0)
        {
            writer.Write((ushort)0);
            return;
        }

        if (values.Length > ushort.MaxValue)
        {
            throw new InvalidOperationException("Runtime 清单数组长度超过 UInt16 上限。");
        }

        writer.Write((ushort)values.Length);
        foreach (var value in values)
        {
            WriteUtf8(writer, value);
        }
    }

    private void WriteInt32Array(BinaryWriter writer, int[] values)
    {
        if (values == null || values.Length == 0)
        {
            writer.Write((ushort)0);
            return;
        }

        if (values.Length > ushort.MaxValue)
        {
            throw new InvalidOperationException("Runtime 清单数组长度超过 UInt16 上限。");
        }

        writer.Write((ushort)values.Length);
        foreach (var value in values)
        {
            writer.Write(value);
        }
    }

    private void LoadCollectorRulesFromDisk()
    {
        EnsureEditorBridge();
        var globalPath = _editorPlatformBridge.GlobalizePath(CollectorRulesSettingsPath);
        string content;
        if (File.Exists(globalPath))
        {
            content = File.ReadAllText(globalPath, Encoding.UTF8);
        }
        else
        {
            content = BuildCollectorRulesJson(BuildDefaultCollectorRulesConfig());
        }

        if (!TryParseCollectorRulesConfig(content, out var config, out var error))
        {
            config = BuildDefaultCollectorRulesConfig();
            content = BuildCollectorRulesJson(config);
            AppendCollectorLog($"{CollectorRulesParseFailedPrefix}{error}");
        }
        else
        {
            content = BuildCollectorRulesJson(config);
        }

        SetCollectorRulesJsonText(content);
        SetStatus(CollectorRulesLoadSuccess);
    }

    private void SaveCollectorRulesToDisk()
    {
        if (_collectorRulesJsonEdit == null)
        {
            return;
        }

        if (!TryParseCollectorRulesConfig(_collectorRulesJsonEdit.Text, out var config, out var error))
        {
            SetStatus($"{CollectorRulesParseFailedPrefix}{error}");
            AppendCollectorLog($"{CollectorRulesParseFailedPrefix}{error}");
            return;
        }

        try
        {
            EnsureEditorBridge();
            var globalPath = _editorPlatformBridge.GlobalizePath(CollectorRulesSettingsPath);
            var directory = Path.GetDirectoryName(globalPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var content = BuildCollectorRulesJson(config);
            File.WriteAllText(globalPath, content, Encoding.UTF8);
            SetCollectorRulesJsonText(content);
            SetStatus(CollectorRulesSaveSuccess);
        }
        catch (Exception ex)
        {
            SetStatus($"{CollectorRulesParseFailedPrefix}{ex.Message}");
            AppendCollectorLog($"{CollectorRulesParseFailedPrefix}{ex.Message}");
        }
    }

    private void ResetCollectorRulesToDefault()
    {
        var config = BuildDefaultCollectorRulesConfig();
        SetCollectorRulesJsonText(BuildCollectorRulesJson(config));
    }

    private void SetCollectorRulesJsonText(string content)
    {
        if (_collectorRulesJsonEdit == null)
        {
            return;
        }

        _collectorRulesSyncing = true;
        _collectorRulesJsonEdit.Text = content;
        _collectorRulesSyncing = false;
    }

    private CollectorRulesConfig BuildDefaultCollectorRulesConfig()
    {
        var config = new CollectorRulesConfig();
        config.Groups.Add(new CollectorRuleGroup
        {
            Name = CollectorRulesDefaultGroupName,
            Enabled = true,
            ScanRoot = string.Empty,
            IncludeExtensions = CollectorDefaultKeywords,
            ExcludeExtensions = string.Empty,
            IncludeKeywords = string.Empty,
            ExcludeKeywords = string.Empty,
            PackageName = string.Empty,
            PackRule = CollectorPackRuleTopDirectory,
            AddressRule = CollectorAddressRulePathNoExt
        });
        return config;
    }

    private static string BuildCollectorRulesJson(CollectorRulesConfig config)
    {
        return JsonSerializer.Serialize(config, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private bool TryParseCollectorRulesConfig(string jsonText, out CollectorRulesConfig config, out string errorMessage)
    {
        errorMessage = string.Empty;
        config = BuildDefaultCollectorRulesConfig();
        if (string.IsNullOrWhiteSpace(jsonText))
        {
            return true;
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<CollectorRulesConfig>(jsonText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (parsed == null)
            {
                errorMessage = "JSON 为空。";
                return false;
            }

            NormalizeCollectorRulesConfig(parsed);
            config = parsed;
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }

    private void NormalizeCollectorRulesConfig(CollectorRulesConfig config)
    {
        config.Groups ??= new List<CollectorRuleGroup>();
        for (var index = 0; index < config.Groups.Count; index++)
        {
            var group = config.Groups[index] ?? new CollectorRuleGroup();
            if (string.IsNullOrWhiteSpace(group.Name))
            {
                group.Name = $"{CollectorRulesDefaultGroupName}_{index + 1}";
            }

            if (string.IsNullOrWhiteSpace(group.PackRule))
            {
                group.PackRule = CollectorPackRuleTopDirectory;
            }

            if (string.IsNullOrWhiteSpace(group.AddressRule))
            {
                group.AddressRule = CollectorAddressRulePathNoExt;
            }

            config.Groups[index] = group;
        }
    }

    private void RunCollectorPrototype()
    {
        var scanRoot = _collectorScanRootInput?.Text?.Trim() ?? string.Empty;
        var keywordsText = _collectorKeywordInput?.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(scanRoot))
        {
            SetStatus(CollectorParamErrorScanRootStatus);
            AppendCollectorLog(CollectorParamErrorScanRootLog);
            return;
        }

        var pipeline = _builderPipelineOptions == null
            ? ReporterDefaultPipeline
            : NormalizeBuilderPipeline(_builderPipelineOptions.GetItemText(_builderPipelineOptions.Selected));
        if (string.IsNullOrEmpty(keywordsText))
        {
            keywordsText = GetDefaultPipelineKeywords(pipeline);
            if (_collectorKeywordInput != null)
            {
                _collectorKeywordInput.Text = keywordsText;
            }
        }

        if (!TryParseCollectorRulesConfig(_collectorRulesJsonEdit?.Text ?? string.Empty, out var config, out var parseError))
        {
            SetStatus($"{CollectorRulesParseFailedPrefix}{parseError}");
            AppendCollectorLog($"{CollectorRulesParseFailedPrefix}{parseError}");
            return;
        }

        NormalizeCollectorRulesConfig(config);
        var enabledGroups = config.Groups.Where(group => group.Enabled).ToList();
        if (_collectorPreviewView != null)
        {
            _collectorPreviewView.Clear();
        }

        AppendCollectorLog(CollectorRunLogStart);
        AppendCollectorLog($"{CollectorRunLogScanRootPrefix}{scanRoot}");
        AppendCollectorLog($"{CollectorRunLogPipelinePrefix}{pipeline}");
        AppendCollectorLog($"{CollectorRunLogKeywordsPrefix}{keywordsText}");
        AppendCollectorLog($"{CollectorRunLogEnabledGroupCountPrefix}{enabledGroups.Count}");
        if (enabledGroups.Count == 0)
        {
            SetStatus($"{CollectorRunStatusCompletedPrefix}0{CollectorRunStatusCompletedSuffix}");
            return;
        }

        var totalMatched = 0;
        var totalScanned = 0;
        var scanLimitHit = false;
        var previewCount = 0;
        for (var index = 0; index < enabledGroups.Count; index++)
        {
            var group = enabledGroups[index];
            var groupName = string.IsNullOrWhiteSpace(group.Name) ? $"{CollectorRulesDefaultGroupName}_{index + 1}" : group.Name.Trim();
            var groupScanRoot = string.IsNullOrWhiteSpace(group.ScanRoot) ? scanRoot : group.ScanRoot.Trim();
            var groupRule = BuildCollectorRuleModelFromGroup(group, keywordsText);
            if (!TryCollectFiles(groupScanRoot, groupRule, pipeline, CollectorScanLimit, out var globalScanRoot, out var matchedFiles, out var scannedCount, out var groupScanLimitHit, out var errorMessage))
            {
                AppendCollectorLog($"{CollectorRunLogGroupPrefix}{groupName}: {errorMessage}");
                continue;
            }

            totalMatched += matchedFiles.Count;
            totalScanned += Math.Min(scannedCount, CollectorScanLimit);
            scanLimitHit |= groupScanLimitHit;
            AppendCollectorLog($"{CollectorRunLogGroupPrefix}{groupName}: {CollectorRunLogMatchedCountPrefix}{matchedFiles.Count}");
            for (var fileIndex = 0; fileIndex < matchedFiles.Count && previewCount < CollectorPreviewLimit; fileIndex++)
            {
                var filePath = matchedFiles[fileIndex];
                var packageName = ResolveCollectorPackageName(group, filePath, globalScanRoot);
                var address = ResolveCollectorAddress(group, filePath);
                var displayPath = ToProjectDisplayPath(filePath);
                _collectorPreviewView?.AppendText($"[{groupName}] {CollectorRunLogPackagePrefix}{packageName} {CollectorRunLogAddressPrefix}{address} {CollectorRunLogPathPrefix}{displayPath}{PluginLogLineTerminator}");
                previewCount++;
            }
        }

        AppendCollectorLog($"{CollectorRunLogScannedCountPrefix}{totalScanned}");
        AppendCollectorLog($"{CollectorRunLogMatchedCountPrefix}{totalMatched}");
        AppendCollectorLog($"{CollectorRunLogPreviewLimitPrefix}{CollectorPreviewLimit}");
        if (scanLimitHit)
        {
            AppendCollectorLog($"{CollectorRunLogScanLimitHitPrefix}{CollectorScanLimit}");
        }

        SetStatus($"{CollectorRunStatusCompletedPrefix}{totalMatched}{CollectorRunStatusCompletedSuffix}");
    }

    private static CollectorRuleModel BuildCollectorRuleModelFromGroup(CollectorRuleGroup group, string fallbackKeywordsText)
    {
        var model = ParseCollectorRuleModel(fallbackKeywordsText);
        if (group == null)
        {
            return model;
        }

        AppendCollectorExtensions(group.IncludeExtensions, model.IncludeExtensions);
        AppendCollectorExtensions(group.ExcludeExtensions, model.ExcludeExtensions);
        AppendCollectorKeywords(group.IncludeKeywords, model.IncludeKeywords);
        AppendCollectorKeywords(group.ExcludeKeywords, model.ExcludeKeywords);
        return model;
    }

    private static void AppendCollectorExtensions(string content, HashSet<string> target)
    {
        foreach (var token in SplitCollectorTokens(content))
        {
            var extension = NormalizeCollectorExtension(token);
            if (!string.IsNullOrEmpty(extension))
            {
                target.Add(extension);
            }
        }
    }

    private static void AppendCollectorKeywords(string content, List<string> target)
    {
        foreach (var token in SplitCollectorTokens(content))
        {
            target.Add(token);
        }
    }

    private static IEnumerable<string> SplitCollectorTokens(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return Array.Empty<string>();
        }

        return content.Split(KeywordSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }

    private string ResolveCollectorPackageName(CollectorRuleGroup group, string filePath, string groupRoot)
    {
        if (!string.IsNullOrWhiteSpace(group.PackageName))
        {
            return group.PackageName.Trim();
        }

        var rule = group.PackRule?.Trim() ?? CollectorPackRuleTopDirectory;
        if (string.Equals(rule, CollectorPackRuleFileName, StringComparison.OrdinalIgnoreCase))
        {
            return Path.GetFileNameWithoutExtension(filePath);
        }

        if (string.Equals(rule, CollectorPackRuleTopDirectory, StringComparison.OrdinalIgnoreCase))
        {
            var relativePath = NormalizePathForDisplay(Path.GetRelativePath(groupRoot, filePath));
            var separatorIndex = relativePath.IndexOf('/');
            if (separatorIndex > 0)
            {
                return relativePath.Substring(0, separatorIndex);
            }

            var firstSegment = relativePath.Trim();
            if (!string.IsNullOrEmpty(firstSegment))
            {
                return firstSegment;
            }
        }

        var fallback = _builderPackageNameInput?.Text?.Trim();
        return string.IsNullOrEmpty(fallback) ? BuilderDefaultPackageName : fallback;
    }

    private string ResolveCollectorAddress(CollectorRuleGroup group, string filePath)
    {
        var rule = group.AddressRule?.Trim() ?? CollectorAddressRulePathNoExt;
        var projectPath = ToProjectDisplayPath(filePath);
        if (string.Equals(rule, CollectorAddressRuleFileName, StringComparison.OrdinalIgnoreCase))
        {
            return Path.GetFileNameWithoutExtension(filePath);
        }

        if (string.Equals(rule, CollectorAddressRulePath, StringComparison.OrdinalIgnoreCase))
        {
            return projectPath;
        }

        return TrimPathExtension(projectPath);
    }

    private static string TrimPathExtension(string path)
    {
        var extension = Path.GetExtension(path);
        if (string.IsNullOrEmpty(extension))
        {
            return path;
        }

        return path.Substring(0, path.Length - extension.Length);
    }

    private string ToProjectDisplayPath(string absolutePath)
    {
        var projectRoot = _editorPlatformBridge.GlobalizePath(ProjectDisplayPrefix).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (absolutePath.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
        {
            var relative = absolutePath.Substring(projectRoot.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return $"{ProjectDisplayPrefix}{NormalizePathForDisplay(relative)}";
        }

        return NormalizePathForDisplay(absolutePath);
    }

    private static string NormalizePathForDisplay(string path)
    {
        return path.Replace('\\', '/');
    }

    private void AppendCollectorLog(string message)
    {
        AppendLogEntry(_collectorLogView, message);
    }

    private void RunReporterPrototype()
    {
        var scanRoot = _reporterScanRootInput?.Text?.Trim() ?? string.Empty;
        var keyword = _reporterKeywordInput?.Text?.Trim() ?? string.Empty;
        var pipeline = _builderPipelineOptions == null
            ? ReporterDefaultPipeline
            : NormalizeBuilderPipeline(_builderPipelineOptions.GetItemText(_builderPipelineOptions.Selected));
        var sortMode = _reporterSortOptions == null ? ReporterDefaultSortMode : _reporterSortOptions.GetItemText(_reporterSortOptions.Selected);
        if (string.IsNullOrEmpty(scanRoot))
        {
            scanRoot = _collectorScanRootInput?.Text?.Trim() ?? ReporterDefaultScanRoot;
        }

        var keywordsText = keyword;
        if (string.IsNullOrEmpty(keywordsText))
        {
            keywordsText = _collectorKeywordInput?.Text?.Trim() ?? string.Empty;
        }

        if (string.IsNullOrEmpty(keywordsText))
        {
            keywordsText = GetDefaultPipelineKeywords(pipeline);
        }

        var scanLimit = ReporterScanLimit;
        if (!TryCollectFiles(scanRoot, keywordsText, pipeline, scanLimit, out var globalScanRoot, out var matchedFiles, out var scannedCount, out var scanLimitHit, out var errorMessage))
        {
            SetStatus(ReporterRunStatusScanRootUnavailable);
            AppendReporterLog($"{ReporterRunLogFailedPrefix}{errorMessage}");
            return;
        }

        var extensionMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in matchedFiles)
        {
            var extension = Path.GetExtension(file);
            if (string.IsNullOrEmpty(extension))
            {
                extension = ReporterNoExtensionPlaceholder;
            }

            if (!extensionMap.TryAdd(extension, 1))
            {
                extensionMap[extension]++;
            }
        }

        var lines = BuildExtensionSummary(extensionMap, sortMode, ReporterSummaryTopExtensionCount);
        _reporterLastSummary = BuildReporterSummaryText(scanRoot, globalScanRoot, pipeline, keyword, keywordsText, sortMode, scannedCount, scanLimit, matchedFiles.Count, lines, scanLimitHit);
        _reporterSummaryFields = ParseSummaryFields(_reporterLastSummary);

        if (_reporterSummaryView != null)
        {
            _reporterSummaryView.Clear();
            _reporterSummaryView.AppendText(_reporterLastSummary);
        }

        AppendReporterLog(ReporterRunLogStart);
        AppendReporterLog($"{ReporterRunLogScanRootPrefix}{scanRoot}");
        AppendReporterLog($"{ReporterRunLogPipelinePrefix}{pipeline}");
        AppendReporterLog($"{ReporterRunLogFilterKeywordPrefix}{keyword}");
        AppendReporterLog($"{ReporterRunLogCollectorKeywordsPrefix}{keywordsText}");
        AppendReporterLog($"{ReporterRunLogSortModePrefix}{sortMode}");
        AppendReporterLog($"{ReporterRunLogScannedCountPrefix}{Math.Min(scannedCount, scanLimit)}");
        AppendReporterLog($"{ReporterRunLogMatchedCountPrefix}{matchedFiles.Count}");
        SetStatus($"{ReporterRunStatusCompletedPrefix}{matchedFiles.Count}{ReporterRunStatusCompletedSuffix}");
    }

    private void LoadLatestBuildReport()
    {
        AppendReporterLog(ReporterLoadBuildReportLogStart);
        if (!TryResolveLatestBuildPackageRoot(out var packageRoot))
        {
            SetStatus(ReporterLoadBuildReportUnavailable);
            AppendReporterLog(ReporterLoadBuildReportUnavailable);
            return;
        }

        var manifestPath = Path.Combine(packageRoot, BuilderManifestFileName);
        var versionPath = Path.Combine(packageRoot, BuilderVersionFileName);
        var manifestAvailable = File.Exists(manifestPath);
        var versionAvailable = File.Exists(versionPath);
        if (!manifestAvailable)
        {
            AppendReporterLog(ReporterLoadBuildReportMissingManifest);
        }

        if (!versionAvailable)
        {
            AppendReporterLog(ReporterLoadBuildReportMissingVersion);
        }

        var manifestFields = ReadKeyValueFile(manifestPath);
        var versionFields = ReadKeyValueFile(versionPath);
        var packageName = GetBuildField(versionFields, manifestFields, ManifestFieldPackageName);
        var packageVersion = GetBuildField(versionFields, manifestFields, ManifestFieldBuildVersion);
        var pipeline = GetBuildField(versionFields, manifestFields, ManifestFieldPipeline);
        var runtimeVersionFilePath = string.IsNullOrEmpty(packageName) ? string.Empty : Path.Combine(packageRoot, YooAssetSettingsData.GetPackageVersionFileName(packageName));
        var runtimeManifestFilePath = string.IsNullOrEmpty(packageName) || string.IsNullOrEmpty(packageVersion)
            ? string.Empty
            : Path.Combine(packageRoot, YooAssetSettingsData.GetManifestBinaryFileName(packageName, packageVersion));
        var runtimeHashFilePath = string.IsNullOrEmpty(packageName) || string.IsNullOrEmpty(packageVersion)
            ? string.Empty
            : Path.Combine(packageRoot, YooAssetSettingsData.GetPackageHashFileName(packageName, packageVersion));
        var runtimeVersionAvailable = !string.IsNullOrEmpty(runtimeVersionFilePath) && File.Exists(runtimeVersionFilePath);
        var runtimeManifestAvailable = !string.IsNullOrEmpty(runtimeManifestFilePath) && File.Exists(runtimeManifestFilePath);
        var runtimeHashAvailable = !string.IsNullOrEmpty(runtimeHashFilePath) && File.Exists(runtimeHashFilePath);
        if (!runtimeVersionAvailable)
        {
            AppendReporterLog(ReporterLoadBuildReportRuntimeVersionMissing);
        }

        if (!runtimeHashAvailable)
        {
            AppendReporterLog(ReporterLoadBuildReportRuntimeHashMissing);
        }

        if (!runtimeManifestAvailable)
        {
            AppendReporterLog(ReporterLoadBuildReportRuntimeManifestMissing);
        }

        var expectedHash = runtimeHashAvailable ? File.ReadAllText(runtimeHashFilePath).Trim() : string.Empty;
        var actualHash = runtimeManifestAvailable ? HashUtility.FileMD5(runtimeManifestFilePath) : string.Empty;
        var runtimeHashMatch = runtimeHashAvailable && runtimeManifestAvailable && string.Equals(expectedHash, actualHash, StringComparison.OrdinalIgnoreCase);
        if (runtimeHashAvailable && runtimeManifestAvailable && !runtimeHashMatch)
        {
            AppendReporterLog(ReporterLoadBuildReportRuntimeHashMismatch);
        }

        var runtimeVersionValue = runtimeVersionAvailable ? File.ReadAllText(runtimeVersionFilePath).Trim() : string.Empty;
        var runtimeVersionMatch = runtimeVersionAvailable && !string.IsNullOrEmpty(packageVersion) && string.Equals(runtimeVersionValue, packageVersion, StringComparison.Ordinal);
        if (runtimeVersionAvailable && !string.IsNullOrEmpty(packageVersion) && !runtimeVersionMatch)
        {
            AppendReporterLog(ReporterLoadBuildReportRuntimeVersionMismatch);
        }

        _lastBuildOutputDirectory = packageRoot;
        _lastBuildManifestPath = manifestPath;
        _lastBuildPackageName = packageName;
        _lastBuildPipeline = pipeline;
        _lastBuildFileCount = ParseIntField(GetBuildField(versionFields, manifestFields, ManifestFieldFileCount));
        _lastBuildScannedCount = _lastBuildFileCount;
        _lastBuildScanLimitHit = false;
        _reporterLastSummary = BuildBuildReportSummaryText(packageRoot, packageName, packageVersion, pipeline, manifestAvailable, versionAvailable, runtimeVersionAvailable, runtimeHashAvailable, runtimeManifestAvailable, runtimeHashMatch, runtimeVersionMatch, expectedHash, actualHash);
        _reporterSummaryFields = ParseSummaryFields(_reporterLastSummary);
        if (_reporterSummaryView != null)
        {
            _reporterSummaryView.Clear();
            _reporterSummaryView.AppendText(_reporterLastSummary);
        }

        SetStatus(ReporterLoadBuildReportCompleted);
        AppendReporterLog($"{ReporterOpenManifestLogPrefix}{NormalizePathForDisplay(manifestPath)}");
        AppendReporterLog(ReporterLoadBuildReportCompleted);
    }

    private void ExportReporterPlaceholder()
    {
        if (string.IsNullOrEmpty(_reporterLastSummary))
        {
            SetStatus(ReporterExportStatusNoSummary);
            AppendReporterLog(ReporterExportLogNoSummary);
            return;
        }

        try
        {
            var folder = _editorPlatformBridge.GlobalizePath(ReporterExportOutputDirectory);
            Directory.CreateDirectory(folder);
            var exportTime = DateTime.Now;
            var textFileName = BuildReporterExportFileName(ExportFileNameTextExtension);
            var jsonFileName = BuildReporterExportFileName(ExportFileNameJsonExtension);
            var textFullPath = Path.Combine(folder, textFileName);
            var jsonFullPath = Path.Combine(folder, jsonFileName);
            File.WriteAllText(textFullPath, BuildReporterExportText(textFileName, exportTime), Encoding.UTF8);
            File.WriteAllText(jsonFullPath, BuildReporterExportJsonText(jsonFileName, exportTime), Encoding.UTF8);
            ResolveManifestState(out var manifestAvailable, out var manifestUnavailableReason);
            AppendReporterLog($"{ReporterExportLogSuccessPrefix}{textFullPath}");
            AppendReporterLog($"{ReporterExportLogSuccessPrefix}{jsonFullPath}");
            AppendReporterLog(string.Format(ReporterExportStateLogFormat, ReporterExportStateLogPrefix, ExportFieldBuildManifestAvailable, manifestAvailable, ExportFieldBuildManifestUnavailableReason, manifestUnavailableReason));
            SetStatus(ReporterExportStatusSuccess);
        }
        catch (Exception ex)
        {
            AppendReporterLog($"{ReporterExportLogFailedPrefix}{ex.Message}");
            SetStatus(ReporterExportStatusFailed);
        }
    }

    private string BuildReporterExportFileName(string extension)
    {
        var timestamp = DateTime.Now.ToString(ExportFileNameTimestampFormat);
        if (_lastBuildTime != DateTime.MinValue)
        {
            timestamp = _lastBuildTime.ToString(ExportFileNameTimestampFormat);
        }

        var packageName = SanitizeFileNameSegment(_lastBuildPackageName);
        if (string.IsNullOrEmpty(packageName))
        {
            packageName = ExportFileNameUnknownPackage;
        }

        return $"{ExportFileNamePrefix}{ExportFileNameSeparator}{packageName}{ExportFileNameSeparator}{timestamp}{extension}";
    }

    private static string SanitizeFileNameSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim();
        foreach (var invalidChar in Path.GetInvalidFileNameChars())
        {
            normalized = normalized.Replace(invalidChar, FileNameReplacementChar);
        }

        normalized = normalized.Replace(FileNameSpaceChar, FileNameReplacementChar);
        return normalized;
    }

    private void ResolveManifestState(out bool manifestAvailable, out string manifestUnavailableReason)
    {
        if (string.IsNullOrEmpty(_lastBuildOutputDirectory))
        {
            manifestAvailable = false;
            manifestUnavailableReason = ManifestUnavailableNoBuildRecord;
            return;
        }

        manifestAvailable = !string.IsNullOrEmpty(_lastBuildManifestPath) && File.Exists(_lastBuildManifestPath);
        manifestUnavailableReason = manifestAvailable ? ManifestUnavailableNone : ManifestUnavailableNotFound;
    }

    private string BuildReporterExportText(string exportFileName, DateTime exportTime)
    {
        ResolveManifestState(out var manifestAvailable, out var manifestUnavailableReason);
        var builder = new StringBuilder();
        builder.AppendLine(ExportSectionInfo);
        builder.AppendLine($"{ExportFieldExportFileName}={exportFileName}");
        builder.AppendLine($"{ExportFieldExportTime}={exportTime.ToString(ExportIsoDateTimeFormat)}");
        builder.AppendLine($"{ExportFieldBuildManifestAvailable}={manifestAvailable}");
        builder.AppendLine($"{ExportFieldBuildManifestUnavailableReason}={manifestUnavailableReason}");
        builder.AppendLine();
        builder.AppendLine(ExportSectionReporterSummary);
        builder.AppendLine(_reporterLastSummary);
        builder.AppendLine();
        builder.AppendLine(ExportSectionBuilderSnapshot);
        if (string.IsNullOrEmpty(_lastBuildOutputDirectory))
        {
            builder.AppendLine($"{SnapshotFieldState}={ManifestUnavailableNoBuildRecord}");
            builder.AppendLine($"{SnapshotFieldManifestAvailable}=false");
            builder.AppendLine($"{SnapshotFieldManifestUnavailableReason}={ManifestUnavailableNoBuildRecord}");
            builder.AppendLine();
            builder.AppendLine(ExportSectionBuildManifest);
            builder.AppendLine($"{SnapshotFieldState}={ManifestUnavailableNoBuildRecord}");
            return builder.ToString();
        }

        builder.AppendLine($"{SnapshotFieldPackageName}={_lastBuildPackageName}");
        builder.AppendLine($"{SnapshotFieldPipeline}={_lastBuildPipeline}");
        builder.AppendLine($"{SnapshotFieldBuildTime}={_lastBuildTime.ToString(ExportIsoDateTimeFormat)}");
        builder.AppendLine($"{SnapshotFieldOutputDirectory}={NormalizePathForDisplay(_lastBuildOutputDirectory)}");
        builder.AppendLine($"{SnapshotFieldManifestPath}={NormalizePathForDisplay(_lastBuildManifestPath)}");
        builder.AppendLine($"{SnapshotFieldScannedCount}={_lastBuildScannedCount}");
        builder.AppendLine($"{SnapshotFieldFileCount}={_lastBuildFileCount}");
        builder.AppendLine($"{SnapshotFieldScanLimitHit}={_lastBuildScanLimitHit}");
        builder.AppendLine($"{SnapshotFieldManifestAvailable}={manifestAvailable}");
        builder.AppendLine($"{SnapshotFieldManifestUnavailableReason}={manifestUnavailableReason}");
        builder.AppendLine();
        builder.AppendLine(ExportSectionBuildManifest);
        if (manifestAvailable)
        {
            builder.Append(File.ReadAllText(_lastBuildManifestPath));
        }
        else
        {
            builder.AppendLine($"{SnapshotFieldState}={ManifestUnavailableNotFound}");
        }

        return builder.ToString();
    }

    private string BuildReporterExportJsonText(string exportFileName, DateTime exportTime)
    {
        ResolveManifestState(out var manifestAvailable, out var manifestUnavailableReason);
        var manifestContent = string.Empty;
        if (manifestAvailable && !string.IsNullOrEmpty(_lastBuildManifestPath))
        {
            manifestContent = File.ReadAllText(_lastBuildManifestPath);
        }

        var payload = new ReporterExportPayload
        {
            Export = new ReporterExportInfo
            {
                FileName = exportFileName,
                ExportTime = exportTime.ToString(ExportIsoDateTimeFormat),
                BuildManifestAvailable = manifestAvailable,
                BuildManifestUnavailableReason = manifestUnavailableReason
            },
            Summary = new ReporterExportSummary
            {
                Raw = _reporterLastSummary,
                Fields = new Dictionary<string, string>(_reporterSummaryFields, StringComparer.OrdinalIgnoreCase)
            },
            BuildSnapshot = new ReporterBuildSnapshot
            {
                State = string.IsNullOrEmpty(_lastBuildOutputDirectory) ? ManifestUnavailableNoBuildRecord : ManifestUnavailableNone,
                PackageName = _lastBuildPackageName,
                Pipeline = _lastBuildPipeline,
                BuildTime = _lastBuildTime == DateTime.MinValue ? string.Empty : _lastBuildTime.ToString(ExportIsoDateTimeFormat),
                OutputDirectory = NormalizePathForDisplay(_lastBuildOutputDirectory ?? string.Empty),
                ManifestPath = NormalizePathForDisplay(_lastBuildManifestPath ?? string.Empty),
                ScannedCount = _lastBuildScannedCount,
                FileCount = _lastBuildFileCount,
                ScanLimitHit = _lastBuildScanLimitHit,
                ManifestAvailable = manifestAvailable,
                ManifestUnavailableReason = manifestUnavailableReason
            },
            BuildManifest = manifestContent
        };

        return JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private void OpenLastBuildOutputDirectory()
    {
        if (string.IsNullOrEmpty(_lastBuildOutputDirectory) || !Directory.Exists(_lastBuildOutputDirectory))
        {
            SetStatus(ReporterOpenOutputUnavailable);
            AppendReporterLog(ReporterOpenOutputUnavailable);
            return;
        }

        var openPath = NormalizePathForDisplay(_lastBuildOutputDirectory);
        _editorPlatformBridge.OpenExternalPath(openPath);
        AppendReporterLog($"{ReporterOpenOutputLogPrefix}{openPath}");
        SetStatus(ReporterOpenOutputSuccess);
    }

    private void OpenLastBuildManifestFile()
    {
        if (string.IsNullOrEmpty(_lastBuildManifestPath) || !File.Exists(_lastBuildManifestPath))
        {
            SetStatus(ReporterOpenManifestUnavailable);
            AppendReporterLog(ReporterOpenManifestUnavailable);
            return;
        }

        var openPath = NormalizePathForDisplay(_lastBuildManifestPath);
        _editorPlatformBridge.OpenExternalPath(openPath);
        AppendReporterLog($"{ReporterOpenManifestLogPrefix}{openPath}");
        SetStatus(ReporterOpenManifestSuccess);
    }

    private static string BuildExtensionSummary(Dictionary<string, int> extensionMap, string sortMode, int topLimit)
    {
        var list = new List<KeyValuePair<string, int>>(extensionMap);
        if (sortMode == ExtensionSummarySortModeAsc)
        {
            list.Sort((a, b) => string.Compare(a.Key, b.Key, StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            list.Sort((a, b) => b.Value.CompareTo(a.Value));
        }

        if (list.Count == 0)
        {
            return ExtensionSummaryEmpty;
        }

        var result = string.Empty;
        for (var i = 0; i < list.Count && i < topLimit; i++)
        {
            result += $"{string.Format(ExtensionSummaryLineFormat, i + 1, list[i].Key, list[i].Value)}{PluginLogLineTerminator}";
        }

        return result.TrimEnd();
    }

    private static string BuildReporterSummaryText(string scanRoot, string globalScanRoot, string pipeline, string keyword, string keywordsText, string sortMode, int scannedCount, int scanLimit, int matchedCount, string extensionLines, bool scanLimitHit)
    {
        var builder = new StringBuilder();
        builder.Append($"{ReporterSummaryScanRootPrefix}{scanRoot}");
        builder.Append($"{ReporterSummaryLineSeparator}{ReporterSummaryGlobalRootPrefix}{globalScanRoot}");
        builder.Append($"{ReporterSummaryLineSeparator}{ReporterSummaryPipelinePrefix}{pipeline}");
        builder.Append($"{ReporterSummaryLineSeparator}{ReporterSummaryFilterKeywordPrefix}{keyword}");
        builder.Append($"{ReporterSummaryLineSeparator}{ReporterSummaryCollectorKeywordsPrefix}{keywordsText}");
        builder.Append($"{ReporterSummaryLineSeparator}{ReporterSummarySortModePrefix}{sortMode}");
        builder.Append($"{ReporterSummaryLineSeparator}{ReporterSummaryScannedFilesPrefix}{Math.Min(scannedCount, scanLimit)}");
        builder.Append($"{ReporterSummaryLineSeparator}{ReporterSummaryMatchedFilesPrefix}{matchedCount}");
        builder.Append($"{ReporterSummaryLineSeparator}{ReporterSummaryTopExtensionsPrefix}");
        builder.Append($"{ReporterSummaryLineSeparator}{extensionLines}");
        if (scanLimitHit)
        {
            builder.Append($"{ReporterSummaryLineSeparator}{ReporterSummaryScanLimitHitPrefix}{scanLimit}");
        }

        return builder.ToString();
    }

    private bool TryResolveLatestBuildPackageRoot(out string packageRoot)
    {
        if (!string.IsNullOrEmpty(_lastBuildManifestPath) && File.Exists(_lastBuildManifestPath))
        {
            packageRoot = Path.GetDirectoryName(_lastBuildManifestPath) ?? string.Empty;
            if (!string.IsNullOrEmpty(packageRoot))
            {
                return true;
            }
        }

        if (!string.IsNullOrEmpty(_lastBuildOutputDirectory) && Directory.Exists(_lastBuildOutputDirectory))
        {
            if (File.Exists(Path.Combine(_lastBuildOutputDirectory, BuilderManifestFileName)))
            {
                packageRoot = _lastBuildOutputDirectory;
                return true;
            }
        }

        var outputPathSetting = _builderOutputPathInput?.Text?.Trim();
        if (string.IsNullOrEmpty(outputPathSetting))
        {
            outputPathSetting = BuilderDefaultOutputPath;
        }

        var globalOutputRoot = _editorPlatformBridge.GlobalizePath(outputPathSetting);
        if (!Directory.Exists(globalOutputRoot))
        {
            packageRoot = string.Empty;
            return false;
        }

        var manifestFiles = Directory.GetFiles(globalOutputRoot, BuilderManifestFileName, SearchOption.AllDirectories);
        if (manifestFiles.Length == 0)
        {
            packageRoot = string.Empty;
            return false;
        }

        Array.Sort(manifestFiles, (left, right) => File.GetLastWriteTimeUtc(right).CompareTo(File.GetLastWriteTimeUtc(left)));
        packageRoot = Path.GetDirectoryName(manifestFiles[0]) ?? string.Empty;
        return !string.IsNullOrEmpty(packageRoot);
    }

    private static Dictionary<string, string> ReadKeyValueFile(string filePath)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            return result;
        }

        foreach (var rawLine in File.ReadAllLines(filePath))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            var splitIndex = line.IndexOf('=');
            if (splitIndex <= 0 || splitIndex >= line.Length - 1)
            {
                continue;
            }

            var key = line.Substring(0, splitIndex).Trim();
            var value = line.Substring(splitIndex + 1).Trim();
            if (!string.IsNullOrEmpty(key))
            {
                result[key] = value;
            }
        }

        return result;
    }

    private static string GetBuildField(Dictionary<string, string> primary, Dictionary<string, string> fallback, string fieldName)
    {
        if (primary != null && primary.TryGetValue(fieldName, out var primaryValue))
        {
            return primaryValue;
        }

        if (fallback != null && fallback.TryGetValue(fieldName, out var fallbackValue))
        {
            return fallbackValue;
        }

        return string.Empty;
    }

    private static int ParseIntField(string value)
    {
        return int.TryParse(value, out var parsed) ? parsed : 0;
    }

    private static string BuildBuildReportSummaryText(string packageRoot, string packageName, string packageVersion, string pipeline, bool manifestAvailable, bool versionAvailable, bool runtimeVersionAvailable, bool runtimeHashAvailable, bool runtimeManifestAvailable, bool runtimeHashMatch, bool runtimeVersionMatch, string expectedHash, string actualHash)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"{ManifestFieldPackageName}={packageName}");
        builder.AppendLine($"{ManifestFieldBuildVersion}={packageVersion}");
        builder.AppendLine($"{ManifestFieldPipeline}={pipeline}");
        builder.AppendLine($"{ReportFieldPackageRoot}={NormalizePathForDisplay(packageRoot)}");
        builder.AppendLine($"{ReportFieldManifestAvailable}={manifestAvailable}");
        builder.AppendLine($"{ReportFieldVersionAvailable}={versionAvailable}");
        builder.AppendLine($"{ReportFieldRuntimeVersionAvailable}={runtimeVersionAvailable}");
        builder.AppendLine($"{ReportFieldRuntimeHashAvailable}={runtimeHashAvailable}");
        builder.AppendLine($"{ReportFieldRuntimeManifestAvailable}={runtimeManifestAvailable}");
        builder.AppendLine($"{ReportFieldRuntimeHashMatch}={runtimeHashMatch}");
        builder.AppendLine($"{ReportFieldRuntimeVersionMatch}={runtimeVersionMatch}");
        builder.AppendLine($"{ReportFieldRuntimeHashValue}={expectedHash}");
        builder.Append($"{ReportFieldRuntimeActualHashValue}={actualHash}");
        return builder.ToString();
    }

    private void AppendReporterLog(string message)
    {
        AppendLogEntry(_reporterLogView, message);
    }

    private void RefreshDebuggerSnapshot()
    {
        var editorHint = _editorPlatformBridge.IsEditorHint();
        var timeScale = _editorPlatformBridge.GetTimeScale();
        var fps = _editorPlatformBridge.GetFramesPerSecond();
        var osName = _editorPlatformBridge.GetOsName();
        var timeScaleText = timeScale.ToString(DebuggerTimeScaleNumberFormat);
        var userPath = _editorPlatformBridge.GlobalizePath(DebuggerPathCheckKeyUser);
        var resPath = _editorPlatformBridge.GlobalizePath(DebuggerPathCheckKeyRes);
        var userExists = Directory.Exists(userPath);
        var resExists = Directory.Exists(resPath);

        if (_debuggerStatusLabel != null)
        {
            _debuggerStatusLabel.Text = string.Format(DebuggerStatusSnapshotFormat, editorHint, fps, timeScaleText);
        }

        if (_debuggerPathLabel != null)
        {
            _debuggerPathLabel.Text = string.Format(DebuggerPathSnapshotFormat, resPath, userPath);
        }

        AppendDebuggerLog(DebuggerLogSnapshotRefreshed);
        AppendDebuggerLog($"{DebuggerLogOsPrefix}{osName}");
        AppendDebuggerLog($"{DebuggerLogEditorHintPrefix}{editorHint}");
        AppendDebuggerLog($"{DebuggerLogFpsPrefix}{fps}");
        AppendDebuggerLog($"{DebuggerLogTimeScalePrefix}{timeScaleText}");
        AppendDebuggerLog($"{DebuggerLogResExistsPrefix}{resExists}");
        AppendDebuggerLog($"{DebuggerLogUserExistsPrefix}{userExists}");
        SetStatus(DebuggerStatusSnapshotRefreshed);
    }

    private void RunDebuggerPathCheck()
    {
        var checks = new Dictionary<string, string>
        {
            [DebuggerPathCheckKeyRes] = _editorPlatformBridge.GlobalizePath(DebuggerPathCheckKeyRes),
            [DebuggerPathCheckKeyUser] = _editorPlatformBridge.GlobalizePath(DebuggerPathCheckKeyUser),
            [DebuggerPathCheckKeyBuilds] = _editorPlatformBridge.GlobalizePath(DebuggerPathCheckKeyBuilds),
            [DebuggerPathCheckKeyReports] = _editorPlatformBridge.GlobalizePath(DebuggerPathCheckKeyReports)
        };

        var passCount = 0;
        foreach (var pair in checks)
        {
            var exists = Directory.Exists(pair.Value);
            if (exists)
            {
                passCount++;
            }

            AppendDebuggerLog(string.Format(DebuggerPathCheckLogFormat, pair.Key, pair.Value, exists));
        }

        SetStatus(string.Format(DebuggerStatusPathCheckFormat, passCount, checks.Count));
    }

    private void RunRuntimeDebugCommand()
    {
        var command = _debuggerCommandInput?.Text?.Trim();
        if (string.IsNullOrEmpty(command))
        {
            command = DebuggerRuntimeCommandDefault;
        }

        var commandParam = _debuggerCommandParamInput?.Text?.Trim() ?? string.Empty;
        AppendDebuggerLog($"{DebuggerLogRuntimeCommandPrefix}{command}");
        AppendDebuggerLog($"{DebuggerLogRuntimeParamPrefix}{commandParam}");

        if (!RemoteCommand.TryParseCommandType(command, out var commandType))
        {
            var invalidMessage = $"Unsupported command \"{command}\"";
            AppendDebuggerLog($"{DebuggerLogRuntimeResultPrefix}{invalidMessage}");
            SetStatus($"{DebuggerStatusRuntimeCommandInvalidPrefix}{command}");
            return;
        }

        var remoteCommand = new RemoteCommand
        {
            CommandType = commandType,
            CommandParam = commandParam
        };
        var commandData = RemoteCommand.Serialize(remoteCommand);
        AppendDebuggerLog($"{DebuggerLogRuntimeProtocolSendBytesPrefix}{commandData.Length}");

        var success = YooAssets.TryExecuteDebugCommand(commandData, out var reportData, out var message);
        if (!success)
        {
            AppendDebuggerLog($"{DebuggerLogRuntimeResultPrefix}{message}");
            SetStatus($"{DebuggerStatusRuntimeCommandFailedPrefix}{message}");
            return;
        }

        AppendDebuggerLog($"{DebuggerLogRuntimeProtocolReceiveBytesPrefix}{reportData.Length}");
        AppendDebuggerLog($"{DebuggerLogRuntimeResultPrefix}{message}");
        var report = DebugReport.Deserialize(reportData);
        _lastRuntimeDebugReport = report;
        AppendRuntimeDebugReport(report);
        SetStatus(DebuggerStatusRuntimeCommandCompleted);
    }

    private void RunDiagnosticFieldAlignmentCheck()
    {
        AppendDebuggerLog(DebuggerLogAlignmentStart);
        var missingFields = new List<string>();
        var invalidFields = new List<string>();
        ValidateReporterSummaryAlignment(missingFields, invalidFields);
        ValidateRuntimeReportAlignment(missingFields, invalidFields);

        for (var index = 0; index < missingFields.Count; index++)
        {
            AppendDebuggerLog($"{DebuggerLogAlignmentMissingPrefix}{missingFields[index]}");
        }

        for (var index = 0; index < invalidFields.Count; index++)
        {
            AppendDebuggerLog($"{DebuggerLogAlignmentInvalidPrefix}{invalidFields[index]}");
        }

        if (missingFields.Count == 0 && invalidFields.Count == 0)
        {
            AppendDebuggerLog(DebuggerLogAlignmentPassed);
            SetStatus(DebuggerStatusAlignmentPassed);
            return;
        }

        SetStatus($"{DebuggerStatusAlignmentFailedPrefix}缺失{missingFields.Count}项，无效{invalidFields.Count}项");
    }

    private void ValidateReporterSummaryAlignment(List<string> missingFields, List<string> invalidFields)
    {
        var requiredFields = new[]
        {
            ManifestFieldPackageName,
            ManifestFieldBuildVersion,
            ManifestFieldPipeline,
            ReportFieldRuntimeVersionAvailable,
            ReportFieldRuntimeHashAvailable,
            ReportFieldRuntimeManifestAvailable,
            ReportFieldRuntimeHashMatch,
            ReportFieldRuntimeVersionMatch
        };

        for (var index = 0; index < requiredFields.Length; index++)
        {
            var field = requiredFields[index];
            if (!_reporterSummaryFields.TryGetValue(field, out var value))
            {
                missingFields.Add($"Reporter.{field}");
                continue;
            }

            if ((field == ManifestFieldPackageName || field == ManifestFieldBuildVersion || field == ManifestFieldPipeline) && string.IsNullOrWhiteSpace(value))
            {
                invalidFields.Add($"Reporter.{field}");
            }
        }
    }

    private void ValidateRuntimeReportAlignment(List<string> missingFields, List<string> invalidFields)
    {
        if (_lastRuntimeDebugReport == null)
        {
            missingFields.Add("Runtime.DebugReport");
            return;
        }

        if (_lastRuntimeDebugReport.PackageDatas == null)
        {
            missingFields.Add("Runtime.PackageDatas");
            return;
        }

        for (var packageIndex = 0; packageIndex < _lastRuntimeDebugReport.PackageDatas.Count; packageIndex++)
        {
            var packageData = _lastRuntimeDebugReport.PackageDatas[packageIndex];
            if (packageData == null)
            {
                invalidFields.Add($"Runtime.PackageDatas[{packageIndex}]");
                continue;
            }

            if (string.IsNullOrWhiteSpace(packageData.PackageName))
            {
                invalidFields.Add($"Runtime.PackageDatas[{packageIndex}].PackageName");
            }

            if (packageData.ProviderInfos == null)
            {
                missingFields.Add($"Runtime.PackageDatas[{packageIndex}].ProviderInfos");
                continue;
            }

            for (var providerIndex = 0; providerIndex < packageData.ProviderInfos.Count; providerIndex++)
            {
                var provider = packageData.ProviderInfos[providerIndex];
                if (provider == null)
                {
                    invalidFields.Add($"Runtime.PackageDatas[{packageIndex}].ProviderInfos[{providerIndex}]");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(provider.AssetPath))
                {
                    invalidFields.Add($"Runtime.PackageDatas[{packageIndex}].ProviderInfos[{providerIndex}].AssetPath");
                }

                if (string.IsNullOrWhiteSpace(provider.Status))
                {
                    invalidFields.Add($"Runtime.PackageDatas[{packageIndex}].ProviderInfos[{providerIndex}].Status");
                }
            }
        }
    }

    private static Dictionary<string, string> ParseSummaryFields(string text)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrEmpty(text))
        {
            return result;
        }

        var lines = text.Split('\n');
        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index].Trim();
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            var separator = line.IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            var key = line.Substring(0, separator).Trim();
            var value = line.Substring(separator + 1).Trim();
            if (string.IsNullOrEmpty(key))
            {
                continue;
            }

            result[key] = value;
        }

        return result;
    }

    private void AppendRuntimeDebugReport(DebugReport report)
    {
        if (report == null)
        {
            return;
        }

        AppendDebuggerLog($"{DebuggerLogRuntimeFramePrefix}{report.FrameCount}");
        for (var packageIndex = 0; packageIndex < report.PackageDatas.Count; packageIndex++)
        {
            var packageData = report.PackageDatas[packageIndex];
            if (packageData == null)
            {
                continue;
            }

            AppendDebuggerLog($"{DebuggerLogRuntimePackagePrefix}{packageData.PackageName}");
            var providerCount = packageData.ProviderInfos == null ? 0 : packageData.ProviderInfos.Count;
            AppendDebuggerLog($"{DebuggerLogRuntimeProviderCountPrefix}{providerCount}");
            if (packageData.ProviderInfos == null)
            {
                continue;
            }

            for (var providerIndex = 0; providerIndex < packageData.ProviderInfos.Count; providerIndex++)
            {
                var providerInfo = packageData.ProviderInfos[providerIndex];
                if (providerInfo == null)
                {
                    continue;
                }

                AppendDebuggerLog(
                    $"{DebuggerLogRuntimeProviderPrefix}{providerInfo.AssetPath}{DebuggerLogRuntimeProviderDetailSeparator}{DebuggerLogRuntimeProviderRefPrefix}{providerInfo.RefCount}{DebuggerLogRuntimeProviderDetailSeparator}{DebuggerLogRuntimeProviderStatusPrefix}{providerInfo.Status}");
            }
        }
    }

    private void AppendDebuggerLog(string message)
    {
        AppendLogEntry(_debuggerLogView, message);
    }

    private void AppendLogEntry(RichTextLabel logView, string message)
    {
        if (logView == null)
        {
            return;
        }

        var time = DateTime.Now.ToString(PluginLogTimeFormat);
        logView.AppendText($"{PluginLogEntryPrefix}{time}{PluginLogEntrySuffix}{message}{PluginLogLineTerminator}");
    }

    private void SwitchTab(int tabIndex, string moduleName)
    {
        if (_tabContainer == null)
        {
            SetStatus(TabSwitchStatusUnavailable);
            return;
        }

        if (tabIndex < 0 || tabIndex >= _tabContainer.GetTabCount())
        {
            SetStatus($"{TabSwitchStatusOutOfRangePrefix}{tabIndex}");
            return;
        }

        _tabContainer.CurrentTab = tabIndex;
        _lastOpenedTabIndex = tabIndex;
        SetStatus($"{TabSwitchStatusPrefix}{moduleName}{TabSwitchStatusSuffix}");
    }

    private void SetStatus(string message)
    {
        if (_statusView == null)
        {
            return;
        }

        _statusView.Clear();
        _statusView.AppendText(message);
    }

    /// <summary>
    /// 编辑器平台能力抽象接口
    /// </summary>
    private interface IEditorPlatformBridge
    {
        string GlobalizePath(string path);
        void OpenExternalPath(string path);
        bool IsEditorHint();
        double GetTimeScale();
        double GetFramesPerSecond();
        string GetOsName();
    }

    /// <summary>
    /// Godot 编辑器平台最小能力实现
    /// </summary>
    private sealed class GodotEditorPlatformBridge : IEditorPlatformBridge
    {
        public string GlobalizePath(string path)
        {
            return ProjectSettings.GlobalizePath(path);
        }

        public void OpenExternalPath(string path)
        {
            OS.ShellOpen(path);
        }

        public bool IsEditorHint()
        {
            return Engine.IsEditorHint();
        }

        public double GetTimeScale()
        {
            return Engine.TimeScale;
        }

        public double GetFramesPerSecond()
        {
            return Engine.GetFramesPerSecond();
        }

        public string GetOsName()
        {
            return OS.GetName();
        }
    }

    private sealed class BuilderPipelineProfile
    {
        public string PipelineName { get; set; }
        public string Description { get; set; }
        public bool SupportsRawFiles { get; set; }
        public bool SupportsCompressionOption { get; set; }
        public int RuntimeOutputNameStyleOverride { get; set; }
        public string DefaultKeywords { get; set; }
        public IReadOnlyList<string> SupportedModes { get; set; } = Array.Empty<string>();
    }

    private sealed class ReporterExportPayload
    {
        public ReporterExportInfo Export { get; set; } = new ReporterExportInfo();
        public ReporterExportSummary Summary { get; set; } = new ReporterExportSummary();
        public ReporterBuildSnapshot BuildSnapshot { get; set; } = new ReporterBuildSnapshot();
        public string BuildManifest { get; set; } = string.Empty;
    }

    private sealed class ReporterExportInfo
    {
        public string FileName { get; set; } = string.Empty;
        public string ExportTime { get; set; } = string.Empty;
        public bool BuildManifestAvailable { get; set; }
        public string BuildManifestUnavailableReason { get; set; } = string.Empty;
    }

    private sealed class ReporterExportSummary
    {
        public string Raw { get; set; } = string.Empty;
        public Dictionary<string, string> Fields { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    private sealed class ReporterBuildSnapshot
    {
        public string State { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public string Pipeline { get; set; } = string.Empty;
        public string BuildTime { get; set; } = string.Empty;
        public string OutputDirectory { get; set; } = string.Empty;
        public string ManifestPath { get; set; } = string.Empty;
        public int ScannedCount { get; set; }
        public int FileCount { get; set; }
        public bool ScanLimitHit { get; set; }
        public bool ManifestAvailable { get; set; }
        public string ManifestUnavailableReason { get; set; } = string.Empty;
    }

    private sealed class BuildExecutionResult
    {
        public string PackageName { get; set; }
        public string Pipeline { get; set; }
        public string PackageVersion { get; set; }
        public string OutputDirectory { get; set; }
        public string ManifestPath { get; set; }
        public int FileCount { get; set; }
        public int ScannedCount { get; set; }
        public bool ScanLimitHit { get; set; }
        public DateTime BuildTime { get; set; }
        public string Summary { get; set; }
    }

    private sealed class BuilderProfileSettings
    {
        public string BuildMode { get; set; }
        public string Encryption { get; set; }
        public string Compression { get; set; }
        public string FileNameStyle { get; set; }
        public string CopyBuildinFileOption { get; set; }
        public string CopyBuildinFileParam { get; set; }
        public string PackageVersion { get; set; }
    }

    private sealed class BuilderBundleEntry
    {
        public string SourceFilePath { get; set; }
        public string BundleName { get; set; }
        public string OutputFileName { get; set; }
        public string DestinationFilePath { get; set; }
        public string FileHash { get; set; }
        public string FileCRC { get; set; }
        public long FileSize { get; set; }
        public bool Encrypted { get; set; }
        public string[] Tags { get; set; } = Array.Empty<string>();
    }

    private sealed class BuildOrchestrationContext
    {
        public string PackageName { get; set; }
        public string GlobalOutputPath { get; set; }
        public string Pipeline { get; set; }
        public string BuildMode { get; set; }
        public string Encryption { get; set; }
        public string Compression { get; set; }
        public string FileNameStyle { get; set; }
        public string CopyBuildinFileOption { get; set; }
        public string CopyBuildinFileParam { get; set; }
        public string RequestedPackageVersion { get; set; }
        public int RuntimeOutputNameStyle { get; set; }
        public bool SkipWriteOutputs { get; set; }
        public IEncryptionServices EncryptionServices { get; set; }
        public string ScanRoot { get; set; }
        public string KeywordsText { get; set; }
        public DateTime BuildTime { get; set; }
        public string BuildTimestamp { get; set; }
        public string PackageVersion { get; set; }
        public string BuildinRoot { get; set; }
        public string SourceRoot { get; set; }
        public string PackageRoot { get; set; }
        public string FilesRoot { get; set; }
        public string ManifestPath { get; set; }
        public string VersionFilePath { get; set; }
        public string RuntimeVersionFilePath { get; set; }
        public string RuntimeManifestFilePath { get; set; }
        public string RuntimeHashFilePath { get; set; }
        public int CopiedCount { get; set; }
        public int ScannedCount { get; set; }
        public bool ScanLimitHit { get; set; }
        public List<string> CollectedFiles { get; set; } = new List<string>();
        public List<BuilderBundleEntry> BundleEntries { get; } = new List<BuilderBundleEntry>();
    }

    private sealed class CollectorRulesConfig
    {
        public List<CollectorRuleGroup> Groups { get; set; } = new List<CollectorRuleGroup>();
    }

    private sealed class CollectorRuleGroup
    {
        public string Name { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
        public string ScanRoot { get; set; } = string.Empty;
        public string IncludeExtensions { get; set; } = string.Empty;
        public string ExcludeExtensions { get; set; } = string.Empty;
        public string IncludeKeywords { get; set; } = string.Empty;
        public string ExcludeKeywords { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public string PackRule { get; set; } = string.Empty;
        public string AddressRule { get; set; } = string.Empty;
    }

    private sealed class CollectorRuleModel
    {
        public readonly HashSet<string> IncludeExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public readonly HashSet<string> ExcludeExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public readonly List<string> IncludeKeywords = new List<string>();
        public readonly List<string> ExcludeKeywords = new List<string>();
    }
}
#endif
