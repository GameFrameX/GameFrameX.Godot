import file_region_replacer
import opcode_spec_parser


def parse_low_level_opcodes(low_level_opcode_spec_file, high_level_opcode_spec_file):
    high_level_opcodes = opcode_spec_parser.parse_high_level_opcode_file(high_level_opcode_spec_file)
    high_level_opcode_dict = {op.name: op for op in high_level_opcodes}
    return opcode_spec_parser.parse_low_level_opcode_file(low_level_opcode_spec_file, high_level_opcode_dict)


def gen_low_level_opcode_enum(low_level_opcodes):
    lines = []
    padding = "    "
    for opcode in low_level_opcodes:
        line = f"{padding}{opcode.name},"
        lines.append(line)
    return "\n".join(lines)

def group_low_level_opcodes_by_prefix(low_level_opcodes):
    grouped = {}
    for opcode in low_level_opcodes:
        hl_name = opcode.prefix
        if hl_name not in grouped:
            grouped[hl_name] = []
        grouped[hl_name].append(opcode)
    return grouped

def gen_low_level_opcode_value(prefix, low_level_opcodes):
    lines = []
    padding = "    "
    for opcode in low_level_opcodes:
        line = f"{padding}{opcode.name} = 0x{opcode.code:02X},"
        lines.append(line)
    if prefix == 0:
        for unused_code in range(len(low_level_opcodes), 0xFB):
            line = f"{padding}__Unused{unused_code:02X} = 0x{unused_code:02X},"
            lines.append(line)
    return "\n".join(lines)

def get_type_cpp_type_name(type_name):
    match type_name:
        case "u8":
            return "uint8_t"
        case "i8":
            return "int8_t"
        case "u16":
            return "uint16_t"
        case "i16":
            return "int16_t"
        case "u32":
            return "uint32_t"
        case "i32":
            return "int32_t"
        case "u64":
            return "uint64_t"
        case "i64":
            return "int64_t"
        case "f32":
            return "float"
        case "f64":
            return "double"
        case _:
            raise NotImplementedError(f"Type {type_name!r} is not supported")

def gen_low_level_opcode_structs(low_level_opcodes):
    lines = []
    for opcode in low_level_opcodes:
        lines.append(f"struct {opcode.name}")
        lines.append("{")
        if opcode.prefix != 0:
            lines.append(f"    uint8_t __prefix;")
        lines.append(f"    uint8_t __code;")
        for param in opcode.params:
            lines.append(f"    {get_type_cpp_type_name(param.type)} {param.name};")
        lines.append("};")
        lines.append("")
    return "\n".join(lines)


def gen_low_level_opcode_size(low_level_opcodes):
    lines = []
    padding = "    "
    for opcode in low_level_opcodes:
        lines.append(f"{padding}sizeof({opcode.name}),")
        # match opcode.name:
        #     case "Switch":
        #         lines.append(f"{padding}OpCodeEnum::{opcode.name} => sizeof(Switch) + (inst.get_num_targets() * sizeof(int32_t)),")
        #     case _:
        #         lines.append(f"{padding}OpCodeEnum::{opcode.name} => sizeof({opcode.name}),")
    return "\n".join(lines)

GENERIC_PARAM_NAMES = ["arg1", "arg2", "arg3", "ret", "res"]

def is_generic_param_name(param_name):
    return param_name in GENERIC_PARAM_NAMES

def gen_low_level_opcode_write_to_data(low_level_opcodes):
    lines = []
    padding = "    "
    for opcode in low_level_opcodes:
        lines.append(f"{padding}case OpCodeEnum::{opcode.name}:")
        lines.append(f"{padding}{{")
        lines.append(f"{padding}    auto ir = ({opcode.name}*)codes;")
        if opcode.prefix != 0:
            lines.append(f"{padding}    ir->__prefix = {opcode_spec_parser.PREFIX_START_CODE + opcode.prefix - 1};")
        lines.append(f"{padding}    ir->__code = {opcode.code};")
        for param in opcode.params:
            if param.name.startswith("__padding_"):
                continue
            if param.arg_kind == "stack":
                lines.append(f"{padding}    ir->{param.name} = ({get_type_cpp_type_name(param.type)})inst.get_var_{param.arg}_eval_stack_idx();")
            # elif is_generic_param_name(param.arg):
            #     lines.append(f"{padding}    ir.{param.name} = inst.get_{param.arg}_const() as {param.type};")
            else:
                lines.append(f"{padding}    ir->{param.name} = ({get_type_cpp_type_name(param.type)})inst.get_{param.arg}();")
        match opcode.name:
            case "Switch":
                lines.append(f"{padding}    auto targetsInfo = inst.get_switch_targets();")
                lines.append(f"{padding}    int32_t self_ir_offset = inst.get_ir_offset();")
                lines.append(f"{padding}    int32_t* target_offsets = (int32_t*)(ir + 1);")
                lines.append(f"{padding}    for (int i = 0; i < targetsInfo.count; i++)")
                lines.append(f"{padding}    {{")
                lines.append(f"{padding}        target_offsets[i] = (int32_t)targetsInfo.targets[i]->ir_offset - self_ir_offset;")
                lines.append(f"{padding}    }}")
                lines.append(f"{padding}    return codes + sizeof({opcode.name}) + (targetsInfo.count * sizeof(int32_t));")
            case _:
                lines.append(f"{padding}    return codes + sizeof({opcode.name});")
        lines.append(f"{padding}}}")
    return "\n".join(lines)


def gen_computed_goto_labels_region(grouped):
    lines = []
    for prefix in range(0, 6):
        opcodes = grouped.get(prefix, [])
        lines.append(f"    static void* const in_labels{prefix}[] = {{")
        for opcode in opcodes:
            lines.append(f"        &&LABEL{prefix}_{opcode.name},")
        if prefix == 0:
            for unused_code in range(len(opcodes), 0xFB):
                lines.append(f"        &&LABEL0___UnusedF9,") # all unused map to same label
            for prefix_id in range(1, 6):
                lines.append(f"        &&LABEL0_Prefix{prefix_id},")
        lines.append(f"    }};")
    return '\n'.join(lines)

# --- 新增：自动生成 prefix0 short 指令 case 代码 ---
import re
def extract_case_blocks_from_interpreter(interpreter_cpp_path):
    # 以20空格缩进的 LEANCLR_CASE_BEGIN... 行为起点，20空格缩进的 LEANCLR_CASE_END... 行为终点，收集所有指令块。
    with open(interpreter_cpp_path, encoding='utf-8') as f:
        lines = f.readlines()
    blocks = {}
    i = 0
    while i < len(lines):
        line = lines[i]
        if line.startswith('                    LEANCLR_CASE_BEGIN'):
            begin_idx = i
            begin_line = line.rstrip('\n')
            # 查找结束行
            j = i + 1
            while j < len(lines):
                if lines[j].startswith('                    LEANCLR_CASE_END'):
                    end_idx = j
                    break
                j += 1
            else:
                # 未找到结尾，跳过
                i += 1
                continue
            block_lines = lines[begin_idx:end_idx+1]
            block = ''.join(block_lines)
            # 提取类型、prefix、指令名
            m = re.match(r'\s*LEANCLR_CASE_BEGIN(_LITE)?([1-5])\((\w+)\)', begin_line)
            if m:
                is_lite = bool(m.group(1))
                prefix = int(m.group(2))
                opname = m.group(3)
                blocks[(opname, prefix, is_lite)] = block
            i = end_idx + 1
        else:
            i += 1
    return blocks


def generate_special_short_instruction_case(short_name):
    match short_name:
        case "InitLocals1Short":
            return """
            LEANCLR_CASE_BEGIN0(InitLocals1Short)
            {
                RtStackObject* locals = eval_stack_base + ir->offset;
                locals->value = 0;
            }
            LEANCLR_CASE_END0()
            """
        case "InitLocals2Short":
            return """
            LEANCLR_CASE_BEGIN0(InitLocals2Short)
            {
                RtStackObject* locals = eval_stack_base + ir->offset;
                locals[0].value = 0;
                locals[1].value = 0;
            }
            LEANCLR_CASE_END0()
            """
        case "InitLocals3Short":
            return """
            LEANCLR_CASE_BEGIN0(InitLocals3Short)
            {
                RtStackObject* locals = eval_stack_base + ir->offset;
                locals[0].value = 0;
                locals[1].value = 0;
                locals[2].value = 0;
            }
            LEANCLR_CASE_END0()
            """
        case "InitLocals4Short":
            return """
            LEANCLR_CASE_BEGIN0(InitLocals4Short)
            {
                RtStackObject* locals = eval_stack_base + ir->offset;
                locals[0].value = 0;
                locals[1].value = 0;
                locals[2].value = 0;
                locals[3].value = 0;
            }
            LEANCLR_CASE_END0()
            """
        case "RetNopShort":
            return """
            LEANCLR_CASE_BEGIN_LITE0(RetNopShort)
            {
                LEAVE_FRAME();
            }
            LEANCLR_CASE_END_LITE0()
            """
        case _:
            return None

def gen_short_instruction_cases(low_level_opcodes, interpreter_cpp_path):
    prefix_blocks = extract_case_blocks_from_interpreter(interpreter_cpp_path)
    blocks = []
    for op in low_level_opcodes:
        if op.prefix != 0:
            continue
        #print(f"Processing short instruction: {op.name}")
        assert(op.name.endswith('Short'))
        long_name = op.name[:-5]
        found = False
        for prefix in range(1, 6):
            key = (long_name, prefix, False)
            block = prefix_blocks.get(key)
            if block:
                block_new = block
                block_new = re.sub(f'LEANCLR_CASE_BEGIN{prefix}\\((\\w+)\\)', lambda m: f'LEANCLR_CASE_BEGIN0({m.group(1)}Short)', block_new)
                block_new = block_new.replace(f'LEANCLR_CASE_END{prefix}()', 'LEANCLR_CASE_END0()')
                block_new = '\n'.join([line[8:] if line.startswith('        ') else line for line in block_new.splitlines()])
                block_new = block_new.replace(f'reinterpret_cast<const ll::{long_name}*>', f'reinterpret_cast<const ll::{long_name}Short*>')
                blocks.append(block_new)
                found = True
                break
            key_lite = (long_name, prefix, True)
            block = prefix_blocks.get(key_lite)
            if block:
                block_new = block
                block_new = re.sub(f'LEANCLR_CASE_BEGIN_LITE{prefix}\\((\\w+)\\)', lambda m: f'LEANCLR_CASE_BEGIN_LITE0({m.group(1)}Short)', block_new)
                block_new = block_new.replace(f'LEANCLR_CASE_END_LITE{prefix}()', 'LEANCLR_CASE_END_LITE0()')
                block_new = '\n'.join([line[8:] if line.startswith('        ') else line for line in block_new.splitlines()])
                block_new = block_new.replace(f'reinterpret_cast<const ll::{long_name}*>', f'reinterpret_cast<const ll::{long_name}Short*>')
                blocks.append(block_new)
                found = True
                break
        if not found:
            block_new = generate_special_short_instruction_case(op.name)
            if block_new:
                blocks.append(block_new)
                found = True
        if not found:
            raise RuntimeError(f"Warning: No matching prefixN case found for short instruction {op.name}")
    # split every line to new lines
    lines = [line for block in blocks for line in block.splitlines() if line.strip() != '']
    #lines = [line for line in lines if line.strip() != '']
    return '\n'.join(lines)

if __name__ == "__main__":
    import sys
    if len(sys.argv) != 6:
        print("Usage: python gen_low_level_opcodes.py <path_to_low_level_opcodes_xml> <path_to_high_level_opcodes_xml> <path_to_ll_opcodes_h> <path_to_ll_opcodes_cpp> <path_to_interpreter_cpp>")
        sys.exit(1)

    low_level_opcode_xml_file = sys.argv[1]
    high_level_opcode_xml_file = sys.argv[2]
    output_file_h = sys.argv[3]
    output_file_cpp = sys.argv[4]
    interpreter_cpp_path = sys.argv[5]
    low_level_opcodes = parse_low_level_opcodes(low_level_opcode_xml_file, high_level_opcode_xml_file)

    frr_h = file_region_replacer.FileRegionReplacer(output_file_h)
    frr_h.replace_region("LOW_LEVEL_OPCODE_ENUM", gen_low_level_opcode_enum(low_level_opcodes))
    grouped_opcodes = group_low_level_opcodes_by_prefix(low_level_opcodes)
    for prefix, opcodes in grouped_opcodes.items():
        frr_h.replace_region(f"LOW_LEVEL_OPCODE{prefix}", gen_low_level_opcode_value(prefix, opcodes))
    frr_h.replace_region("LOW_LEVEL_INSTRUCTION_STRUCTS", gen_low_level_opcode_structs(low_level_opcodes))
    frr_h.save()
    print(f"Updated low-level opcode definitions in {output_file_h}")

    frr_cpp = file_region_replacer.FileRegionReplacer(output_file_cpp)
    frr_cpp.replace_region("LOW_LEVEL_INSTRUCTION_SIZES", gen_low_level_opcode_size(low_level_opcodes))
    frr_cpp.replace_region("LOW_LEVEL_INSTRUCTION_WRITE_TO_DATA", gen_low_level_opcode_write_to_data(low_level_opcodes))
    frr_cpp.save()
    print(f"Updated low-level opcode definitions in {output_file_cpp}")

    # 替换 interpreter.cpp 的 COMPUTED_GOTO_LABELS 区域
    frr_interp = file_region_replacer.FileRegionReplacer(interpreter_cpp_path)
    frr_interp.replace_region("COMPUTED_GOTO_LABELS", gen_computed_goto_labels_region(grouped_opcodes))

    # 替换 SHORT_INSTRUCTION_CASES 区域
    short_cases = gen_short_instruction_cases(low_level_opcodes, interpreter_cpp_path)
    frr_interp.replace_region("SHORT_INSTRUCTION_CASES", short_cases)
    frr_interp.save()
    print(f"Updated COMPUTED_GOTO_LABELS in {interpreter_cpp_path}")

    print(f"Updated SHORT_INSTRUCTION_CASES in {interpreter_cpp_path}")
