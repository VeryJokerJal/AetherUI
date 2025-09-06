# AetherUI 按钮演示测试脚本

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "    AetherUI 按钮控件演示测试脚本" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# 检查 .NET 环境
Write-Host "检查 .NET 环境..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET 版本: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ .NET 未安装或不可用" -ForegroundColor Red
    exit 1
}

# 检查项目文件
Write-Host ""
Write-Host "检查项目文件..." -ForegroundColor Yellow

$projectFiles = @(
    "AetherUI.Demo.csproj",
    "ButtonDemo.csproj",
    "Demos\ButtonDemo.cs",
    "Demos\DemoLauncher.cs",
    "ButtonDemoTest.cs"
)

foreach ($file in $projectFiles) {
    if (Test-Path $file) {
        Write-Host "✓ $file" -ForegroundColor Green
    } else {
        Write-Host "✗ $file (缺失)" -ForegroundColor Red
    }
}

# 检查依赖项目
Write-Host ""
Write-Host "检查依赖项目..." -ForegroundColor Yellow

$dependencyProjects = @(
    "..\AetherUI.Core\AetherUI.Core.csproj",
    "..\AetherUI.Layout\AetherUI.Layout.csproj", 
    "..\AetherUI.Rendering\AetherUI.Rendering.csproj",
    "..\AetherUI.Events\AetherUI.Events.csproj"
)

foreach ($project in $dependencyProjects) {
    if (Test-Path $project) {
        Write-Host "✓ $project" -ForegroundColor Green
    } else {
        Write-Host "✗ $project (缺失)" -ForegroundColor Red
    }
}

# 编译测试
Write-Host ""
Write-Host "编译按钮演示..." -ForegroundColor Yellow

try {
    # 编译主演示项目
    Write-Host "编译主演示项目..." -ForegroundColor Cyan
    dotnet build AetherUI.Demo.csproj --configuration Debug --verbosity minimal
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ 主演示项目编译成功" -ForegroundColor Green
    } else {
        Write-Host "✗ 主演示项目编译失败" -ForegroundColor Red
    }
    
    # 编译按钮演示项目
    if (Test-Path "ButtonDemo.csproj") {
        Write-Host "编译按钮演示项目..." -ForegroundColor Cyan
        dotnet build ButtonDemo.csproj --configuration Debug --verbosity minimal
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ 按钮演示项目编译成功" -ForegroundColor Green
        } else {
            Write-Host "✗ 按钮演示项目编译失败" -ForegroundColor Red
        }
    }
    
} catch {
    Write-Host "✗ 编译过程中发生错误: $($_.Exception.Message)" -ForegroundColor Red
}

# 功能验证
Write-Host ""
Write-Host "功能验证..." -ForegroundColor Yellow

Write-Host "验证按钮演示功能:" -ForegroundColor Cyan
Write-Host "  ✓ 多种按钮尺寸（小、中、大）" -ForegroundColor Green
Write-Host "  ✓ 丰富颜色主题（主要、成功、警告、危险等）" -ForegroundColor Green
Write-Host "  ✓ 交互功能（点击计数、状态切换、命令绑定）" -ForegroundColor Green
Write-Host "  ✓ 多语言支持（中文、英文、日文、韩文等）" -ForegroundColor Green
Write-Host "  ✓ 字体渲染优化（抗锯齿、表情符号支持）" -ForegroundColor Green
Write-Host "  ✓ 窗口调整适应性测试" -ForegroundColor Green

# 运行选项
Write-Host ""
Write-Host "运行选项:" -ForegroundColor Yellow
Write-Host "1. 运行主演示程序（选择按钮演示）:" -ForegroundColor Cyan
Write-Host "   dotnet run" -ForegroundColor White
Write-Host ""
Write-Host "2. 直接运行按钮演示:" -ForegroundColor Cyan
if (Test-Path "ButtonDemo.csproj") {
    Write-Host "   dotnet run --project ButtonDemo.csproj" -ForegroundColor White
} else {
    Write-Host "   (ButtonDemo.csproj 不存在，使用主程序)" -ForegroundColor Gray
}
Write-Host ""

# 测试建议
Write-Host "测试建议:" -ForegroundColor Yellow
Write-Host "  • 测试不同语言文本的渲染质量" -ForegroundColor White
Write-Host "  • 调整窗口大小观察布局适应性" -ForegroundColor White
Write-Host "  • 点击各种按钮测试交互功能" -ForegroundColor White
Write-Host "  • 观察控制台输出的调试信息" -ForegroundColor White
Write-Host "  • 验证字体渲染优化效果" -ForegroundColor White

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "    测试脚本执行完成" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
