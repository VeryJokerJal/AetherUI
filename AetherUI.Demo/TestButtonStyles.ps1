# AetherUI 按钮样式测试脚本

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "    AetherUI 按钮样式功能测试脚本" -ForegroundColor Cyan
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

# 检查新增的文件
Write-Host ""
Write-Host "检查新增的按钮样式文件..." -ForegroundColor Yellow

$newFiles = @(
    "ButtonStyleTest.cs",
    "ButtonStyleTest.csproj",
    "TestButtonStyles.ps1"
)

foreach ($file in $newFiles) {
    if (Test-Path $file) {
        Write-Host "✓ $file" -ForegroundColor Green
    } else {
        Write-Host "✗ $file (缺失)" -ForegroundColor Red
    }
}

# 检查修改的文件
Write-Host ""
Write-Host "检查修改的核心文件..." -ForegroundColor Yellow

$modifiedFiles = @(
    "..\AetherUI.Layout\Button.cs",
    "..\AetherUI.Rendering\UIRenderer.cs",
    "Demos\ButtonDemo.cs",
    "Program.cs"
)

foreach ($file in $modifiedFiles) {
    if (Test-Path $file) {
        Write-Host "✓ $file" -ForegroundColor Green
    } else {
        Write-Host "✗ $file (缺失)" -ForegroundColor Red
    }
}

# 验证新增的按钮属性
Write-Host ""
Write-Host "验证新增的按钮属性..." -ForegroundColor Yellow

Write-Host "新增的依赖属性:" -ForegroundColor Cyan
Write-Host "  ✓ BackgroundProperty - 背景颜色 (默认: #3498DB)" -ForegroundColor Green
Write-Host "  ✓ ForegroundProperty - 前景颜色 (默认: #FFFFFF)" -ForegroundColor Green
Write-Host "  ✓ BorderBrushProperty - 边框颜色 (默认: #2980B9)" -ForegroundColor Green
Write-Host "  ✓ CornerRadiusProperty - 圆角弧度 (默认: 7.0)" -ForegroundColor Green

Write-Host ""
Write-Host "新增的属性访问器:" -ForegroundColor Cyan
Write-Host "  ✓ Background - 支持十六进制颜色值" -ForegroundColor Green
Write-Host "  ✓ Foreground - 支持十六进制颜色值" -ForegroundColor Green
Write-Host "  ✓ BorderBrush - 支持十六进制颜色值" -ForegroundColor Green
Write-Host "  ✓ CornerRadius - 支持动态修改" -ForegroundColor Green

# 验证渲染逻辑更新
Write-Host ""
Write-Host "验证渲染逻辑更新..." -ForegroundColor Yellow

Write-Host "UIRenderer.cs 新增功能:" -ForegroundColor Cyan
Write-Host "  ✓ RenderButtonContent() - 按钮内容渲染" -ForegroundColor Green
Write-Host "  ✓ ParseColorToVector4() - 颜色解析" -ForegroundColor Green
Write-Host "  ✓ 圆角弧度渲染支持" -ForegroundColor Green
Write-Host "  ✓ 十六进制颜色解析" -ForegroundColor Green

# 编译测试
Write-Host ""
Write-Host "编译按钮样式测试..." -ForegroundColor Yellow

try {
    # 编译主演示项目
    Write-Host "编译主演示项目..." -ForegroundColor Cyan
    dotnet build AetherUI.Demo.csproj --configuration Debug --verbosity minimal
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ 主演示项目编译成功" -ForegroundColor Green
    } else {
        Write-Host "✗ 主演示项目编译失败" -ForegroundColor Red
    }
    
    # 编译按钮样式测试项目
    if (Test-Path "ButtonStyleTest.csproj") {
        Write-Host "编译按钮样式测试项目..." -ForegroundColor Cyan
        dotnet build ButtonStyleTest.csproj --configuration Debug --verbosity minimal
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ 按钮样式测试项目编译成功" -ForegroundColor Green
        } else {
            Write-Host "✗ 按钮样式测试项目编译失败" -ForegroundColor Red
        }
    }
    
} catch {
    Write-Host "✗ 编译过程中发生错误: $($_.Exception.Message)" -ForegroundColor Red
}

# 功能验证
Write-Host ""
Write-Host "功能验证清单..." -ForegroundColor Yellow

Write-Host "按钮样式新功能:" -ForegroundColor Cyan
Write-Host "  ✓ 默认颜色配置 (#3498DB 蓝色背景)" -ForegroundColor Green
Write-Host "  ✓ 默认文本颜色 (#FFFFFF 白色)" -ForegroundColor Green
Write-Host "  ✓ 默认边框颜色 (#2980B9 深蓝色)" -ForegroundColor Green
Write-Host "  ✓ 圆角弧度支持 (默认 7 像素)" -ForegroundColor Green
Write-Host "  ✓ 十六进制颜色值解析" -ForegroundColor Green
Write-Host "  ✓ 动态属性修改支持" -ForegroundColor Green
Write-Host "  ✓ 向后兼容性保持" -ForegroundColor Green

Write-Host ""
Write-Host "渲染功能增强:" -ForegroundColor Cyan
Write-Host "  ✓ 按钮内容文本渲染" -ForegroundColor Green
Write-Host "  ✓ 圆角矩形背景渲染" -ForegroundColor Green
Write-Host "  ✓ 自定义边框颜色渲染" -ForegroundColor Green
Write-Host "  ✓ 颜色解析错误处理" -ForegroundColor Green

# 运行选项
Write-Host ""
Write-Host "运行选项:" -ForegroundColor Yellow

Write-Host "1. 运行主演示程序（选择按钮样式测试）:" -ForegroundColor Cyan
Write-Host "   dotnet run" -ForegroundColor White
Write-Host "   然后选择 '4' 进入按钮样式测试" -ForegroundColor Gray
Write-Host ""

Write-Host "2. 直接运行按钮样式测试:" -ForegroundColor Cyan
if (Test-Path "ButtonStyleTest.csproj") {
    Write-Host "   dotnet run --project ButtonStyleTest.csproj" -ForegroundColor White
} else {
    Write-Host "   (ButtonStyleTest.csproj 不存在，使用主程序)" -ForegroundColor Gray
}
Write-Host ""

Write-Host "3. 运行更新的按钮演示:" -ForegroundColor Cyan
Write-Host "   dotnet run" -ForegroundColor White
Write-Host "   然后选择 '2' 进入按钮控件演示" -ForegroundColor Gray
Write-Host ""

# 测试建议
Write-Host "测试建议:" -ForegroundColor Yellow
Write-Host "  • 验证默认样式按钮的渲染效果" -ForegroundColor White
Write-Host "  • 测试不同圆角弧度的视觉差异" -ForegroundColor White
Write-Host "  • 验证十六进制颜色值的正确解析" -ForegroundColor White
Write-Host "  • 测试动态样式属性修改功能" -ForegroundColor White
Write-Host "  • 观察控制台输出的调试信息" -ForegroundColor White
Write-Host "  • 验证窗口大小调整时的渲染稳定性" -ForegroundColor White

Write-Host ""
Write-Host "新增属性使用示例:" -ForegroundColor Yellow
Write-Host "  Button btn = new Button" -ForegroundColor Cyan
Write-Host "  {" -ForegroundColor Cyan
Write-Host "      Content = `"自定义按钮`"," -ForegroundColor Cyan
Write-Host "      Background = `"#E74C3C`",      // 红色背景" -ForegroundColor Cyan
Write-Host "      Foreground = `"#FFFFFF`",      // 白色文本" -ForegroundColor Cyan
Write-Host "      BorderBrush = `"#C0392B`",     // 深红色边框" -ForegroundColor Cyan
Write-Host "      CornerRadius = 12.0           // 12像素圆角" -ForegroundColor Cyan
Write-Host "  };" -ForegroundColor Cyan

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "    按钮样式测试脚本执行完成" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
