import file_region_replacer
import opcode_spec_parser


def gen_high_level_opcode_enum(high_level_opcodes):
    lines = []
    padding = "    "
    for opcode in high_level_opcodes:
        line = f"{padding}{opcode.name},"
        lines.append(line)
    return "\n".join(lines)

if __name__ == "__main__":
    import sys
    if len(sys.argv) != 3:
        print("Usage: python gen_high_level_opcodes.py <path_to_high_level_opcodes_xml> <path_to_output_file>")
        sys.exit(1)

    high_level_opcode_xml_file = sys.argv[1]
    output_file = sys.argv[2]
    high_level_opcodes = opcode_spec_parser.parse_high_level_opcode_file(high_level_opcode_xml_file)
    frr = file_region_replacer.FileRegionReplacer(output_file)
    frr.replace_region("HIGH_LEVEL_OPCODES", gen_high_level_opcode_enum(high_level_opcodes))
    frr.save()
    print(f"Updated high-level opcode definitions in {output_file}")
