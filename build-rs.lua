-- build-rs.lua
print_success("🦀 Сборка проекта xaml-parser-native")

-- Получаем базовую директорию проекта (где лежит lua скрипт)
local base_dir = get_full_path(lua_script_path):match("(.+[/\\])[^/\\]*$")
local rust_project_dir = base_dir .. "xaml-parser-native"
local build_output_dir = base_dir .. "XamlParserConsoleApp\\bin\\Debug\\net9.0"

print_success("📁 Базовая директория: " .. base_dir)
print_success("📁 Rust проект: " .. rust_project_dir)
print_success("📁 Выходная папка: " .. build_output_dir)

-- Переходим в папку с Rust проектом
if not set_cwd(rust_project_dir) then
    print_error("Не удалось перейти в папку: " .. rust_project_dir)
    return
end

-- Проверяем наличие Cargo.toml
if not file_exists("Cargo.toml") then
    print_error("Cargo.toml не найден в " .. rust_project_dir)
    return
end

-- Очищаем предыдущую сборку
print_success("🧹 Очистка предыдущей сборки...")
if dir_exists("target") then
    delete_dir("target")
    println("Очищена папка target")
end

-- Запускаем сборку
print_success("🔨 Запуск сборки в release режиме...")
local timer = create_timer()
timer:start()

local success = task_run("cargo build --release", function(line)
    if contains(line, "Compiling") then
        print_success("📦 " .. line)
    elseif contains(line, "Finished") then
        print_success("✅ " .. line)
    elseif contains(line, "error") or contains(line, "ERROR") then
        print_error("❌ " .. line)
    elseif contains(line, "warning") or contains(line, "WARN") then
        println("⚠️ " .. line)
    else
        println(line)
    end
end)

local elapsed = timer:stop()

if success then
    print_success(string.format("🎉 Сборка завершена за %.2f секунд!", elapsed))
    
    -- Создаем выходную папку Build
    if not dir_exists(build_output_dir) then
        create_dir(build_output_dir)
        print_success("📁 Создана папка: " .. build_output_dir)
    end
    
    -- Копируем артефакты сборки
    local target_release = "target/release"
    if dir_exists(target_release) then
        print_success("📦 Копирование артефактов...")
        
        -- Список возможных артефактов (адаптируйте под ваш проект)
        local artifacts = {
            "xaml_parser_native.dll",
            "libxaml_parser_native.dll", 
            "xaml_parser_native.exe",
            "libxaml_parser_native.so",
            "libxaml_parser_native.dylib"
        }
        
        local copied_count = 0
        for _, artifact in ipairs(artifacts) do
            local source = target_release .. "/" .. artifact
            local dest = build_output_dir .. "/" .. artifact
            
            if file_exists(source) then
                copy_file(source, dest)
                print_success("📁 Скопирован: " .. artifact)
                copied_count = copied_count + 1
            end
        end
        
        if copied_count > 0 then
            print_success(string.format("🎉 Скопировано %d артефактов в Build/", copied_count))
        else
            println("⚠️ Артефакты не найдены, проверьте имена файлов")
        end
    else
        print_error("Папка target/release не найдена!")
    end
    
else
    print_error("💥 Сборка провалена!")
end
