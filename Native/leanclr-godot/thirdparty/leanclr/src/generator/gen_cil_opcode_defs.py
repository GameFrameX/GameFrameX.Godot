import opcode_spec_parser
import file_region_replacer


def gen_cil_opcode_enums(opcodes):
    lines = []
    padding = "    "
    for opcode in opcodes:
        line = f"{padding}{opcode.name2},"
        lines.append(line)
    return "\n".join(lines)

def gen_cil_opcode_values(opcodes):
    lines = []
    padding = "    "
    for opcode in opcodes:
        if opcode.o1 != 0xFF: continue
        line = f"{padding}{opcode.name2} = 0x{opcode.o2:02X},"
        lines.append(line)
    return "\n".join(lines)

def gen_cil_opcode_ext_values(opcodes):
    lines = []
    padding = "    "
    for opcode in opcodes:
        if opcode.o1 != 0xFE: continue
        line = f"{padding}{opcode.name2} = 0x{opcode.o2:02X},"
        lines.append(line)
    return "\n".join(lines)

def calc_arg_type(op_name: str, inline_type: str) -> str:
    match inline_type:
        case "InlineNone":
            match op_name:
                case "ret" | "throw" | "rethrow" | "endfinally":
                    return "StaticBranch"
                case _:
                    return "None"
        case ("ShortInlineVar" | "InlineVar" |
              "ShortInlineI" | "InlineI" | "InlineI8" |
              "ShortInlineR" | "InlineR" |
              "InlineMethod" | "InlineSig" | "InlineType" | "InlineField" | "InlineTok" | "InlineString"):
            return "Data"
        case "ShortInlineBrTarget" | "InlineBrTarget":
            return "BranchTarget"
        case "InlineSwitch":
            return "Switch"
        case _:
            raise NotImplementedError(f"inline_type {inline_type!r} is not supported")

def calc_arg_param(inline_type: str) -> int:
    match inline_type:
        case "InlineNone":
            return 0
        case "ShortInlineVar" | "ShortInlineI" | "ShortInlineBrTarget":
            return 1
        case "InlineVar":
            return 2
        case ("InlineI" | "InlineMethod" | "InlineSig" | "InlineType" |
              "InlineField" | "InlineTok" | "InlineBrTarget" | "InlineString" | "ShortInlineR"):
            return 4
        case "InlineI8" | "InlineR":
            return 8
        case "InlineSwitch":
            return -1
        case _:
            raise NotImplementedError(f"inline_type {inline_type!r} is not supported")

def to_flow_type(s: str) -> str:
    return ''.join(word.capitalize() for word in s.split('-'))

def gen_cil_opcode_metadata_infos(opcodes):
    lines = []
    padding = "    "
    for opcode in opcodes:
        constant_value = opcode.constant if opcode.constant is not None else 0
        line = (f"{padding}{{OpCodeEnum::{opcode.name2}, ArgType::{calc_arg_type(opcode.name, opcode.args)}, "
                f"{calc_arg_param(opcode.args)}, 0x{opcode.o1:02X}, 0x{opcode.o2:02X}, "
                f"FlowType::{to_flow_type(opcode.flow)}, {constant_value}}},")
        lines.append(line)
    return "\n".join(lines)

if __name__ == "__main__":
    import sys
    if len(sys.argv) != 4:
        print("Usage: python gen_cil_opcode_defs.py <path_to_cil_opcodes_xml> <path_to_opcode_h> <path_to_opcode_cpp>")
        sys.exit(1)

    cli_opcode_xml_file = sys.argv[1]
    file_path_h = sys.argv[2]
    file_path_cpp = sys.argv[3]
    opcodes = opcode_spec_parser.parse_cli_opcode_file(cli_opcode_xml_file)

    frr = file_region_replacer.FileRegionReplacer(file_path_h)
    frr.replace_region("OPCODE_ENUM", gen_cil_opcode_enums(opcodes))
    frr.replace_region("OPCODE_VALUE", gen_cil_opcode_values(opcodes))
    frr.replace_region("OPCODE_VALUE_EXT", gen_cil_opcode_ext_values(opcodes))
    frr.save()
    print(f"Updated opcode definitions in {file_path_h}")

    ffr = file_region_replacer.FileRegionReplacer(file_path_cpp)
    ffr.replace_region("OPCODE_INFO", gen_cil_opcode_metadata_infos(opcodes))
    ffr.save()
    print(f"Updated opcode definitions in {file_path_cpp}")
