﻿using System;
using System.IO;
using PascalABCCompiler.ParserTools;
using PascalABCCompiler.SyntaxTree;
using PascalABCCompiler.Parsers;
using System.Resources;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

namespace PascalABCCompiler.PascalABCParser
{
    public class CommentInfo
    {
        public string before;
        public string after;
    }

    public class CommentBinder : IVisitor
    {
        private string Text=null;
        private Dictionary<syntax_tree_node, string[]> comment_table = new Dictionary<syntax_tree_node, string[]>();

        public Dictionary<syntax_tree_node,string[]> Bind(compilation_unit cu, string Text)
        {
            return null;
        }

        private string GetCommentsBefore(int pos)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder tmp = new StringBuilder();
            while (pos >= 0)
            {
                if (char.IsWhiteSpace(Text[pos]))
                {
                    tmp.Insert(0, Text[pos]);
                    pos--;
                }
                else if (Text[pos] == '}')
                {
                    sb.Append(tmp.ToString());
                    while (pos >= 0 && Text[pos] != '{')
                    {
                        sb.Insert(0, Text[pos]);
                        pos--;
                    }
                }
                else
                    break;
            }
            if (sb.Length > 0)
                return sb.ToString();
            else
                return null;
        }

        private string GetCommentsAfter(int pos)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder tmp = new StringBuilder();
            while (pos >= 0)
            {
                if (char.IsWhiteSpace(Text[pos]))
                {
                    tmp.Append(Text[pos]);
                    pos++;
                }
                else if (Text[pos] == '}')
                {
                    sb.Append(tmp.ToString());
                    while (pos >= 0 && Text[pos] != '{')
                    {
                        sb.Append(Text[pos]);
                        pos++;
                    }
                }
                else
                    break;
            }
            if (sb.Length > 0)
                return sb.ToString();
            else
                return null;
        }

        private void AddPossibleComments(syntax_tree_node sn, bool search_before, bool search_after)
        {
            string before = null;
            if (search_before)
                before = GetCommentsBefore(sn.source_context.Position - 1);
            string after = null;
            if (search_after)
                after = GetCommentsAfter(sn.source_context.Position + sn.source_context.Length);
            if (before != null || after != null)
            {
                comment_table.Add(sn, new string[] { before, after });
            }
        }

        #region IVisitor Member

        public void visit(syntax_tree_node _syntax_tree_node)
        {
            throw new NotImplementedException();
        }

        private bool is_begin(statement_list sl)
        {
            return (sl.left_logical_bracket != null && sl.left_logical_bracket is token_info
                    && (sl.left_logical_bracket as token_info).text.ToLower() == "begin");
        }

        public void visit(statement_list _statement_list)
        {
            if (is_begin(_statement_list))
            AddPossibleComments(_statement_list, true, false);
            foreach (statement stmt in _statement_list.subnodes)
                stmt.visit(this);
        }

        public void visit(expression _expression)
        {
            throw new NotImplementedException();
        }

        public void visit(assign _assign)
        {
            _assign.to.visit(this);
            _assign.from.visit(this);
        }

        public void visit(bin_expr _bin_expr)
        {
            _bin_expr.left.visit(this);
            _bin_expr.right.visit(this);
        }

        public void visit(un_expr _un_expr)
        {
            _un_expr.subnode.visit(this);
        }

        public void visit(const_node _const_node)
        {
            throw new NotImplementedException();
        }

        public void visit(bool_const _bool_const)
        {
            AddPossibleComments(_bool_const, true, true);
        }

        public void visit(int32_const _int32_const)
        {
            AddPossibleComments(_int32_const, true, true);
        }

        public void visit(double_const _double_const)
        {
            AddPossibleComments(_double_const, true, true);
        }

        public void visit(statement _statement)
        {
            throw new NotImplementedException();
        }

        public void visit(subprogram_body _subprogram_body)
        {
            throw new NotImplementedException();
        }

        public void visit(ident _ident)
        {
            AddPossibleComments(_ident, true, true);
        }

        public void visit(addressed_value _addressed_value)
        {
            throw new NotImplementedException();
        }

        public void visit(type_definition _type_definition)
        {
            throw new NotImplementedException();
        }

        public void visit(named_type_reference _named_type_reference)
        {
            foreach (ident id in _named_type_reference.names)
                id.visit(this);
        }

        public void visit(variable_definitions _variable_definitions)
        {
            AddPossibleComments(_variable_definitions,true,false);
            foreach (var_def_statement vds in _variable_definitions.var_definitions)
                vds.visit(this);
        }

        public void visit(ident_list _ident_list)
        {
            foreach (ident id in _ident_list.idents)
                id.visit(this);
        }

        public void visit(var_def_statement _var_def_statement)
        {
            if (_var_def_statement.is_event)
                AddPossibleComments(_var_def_statement, true, false);
            _var_def_statement.vars.visit(this);
            if (_var_def_statement.vars_type != null)
                _var_def_statement.vars_type.visit(this);
            if (_var_def_statement.inital_value != null)
                _var_def_statement.inital_value.visit(this);
        }

        public void visit(declaration _declaration)
        {
            throw new NotImplementedException();
        }

        public void visit(declarations _declarations)
        {
            foreach (declaration decl in _declarations.defs)
                decl.visit(this);
        }

        public void visit(program_tree _program_tree)
        {
            throw new NotImplementedException();
        }

        public void visit(program_name _program_name)
        {
            AddPossibleComments(_program_name, true, false);
            _program_name.prog_name.visit(this);
        }

        public void visit(string_const _string_const)
        {
            AddPossibleComments(_string_const, true, true);
        }

        public void visit(expression_list _expression_list)
        {
            foreach (expression ex in _expression_list.expressions)
                ex.visit(this);
        }

        public void visit(dereference _dereference)
        {
            throw new NotImplementedException();
        }

        public void visit(roof_dereference _roof_dereference)
        {
            _roof_dereference.dereferencing_value.visit(this);
            AddPossibleComments(_roof_dereference, false, true);
        }

        public void visit(indexer _indexer)
        {
            _indexer.dereferencing_value.visit(this);
            _indexer.indexes.visit(this);
        }

        public void visit(for_node _for_node)
        {
            AddPossibleComments(_for_node, true, false);
            _for_node.loop_variable.visit(this);
            if (_for_node.type_name != null)
                _for_node.type_name.visit(this);
            _for_node.initial_value.visit(this);
            _for_node.finish_value.visit(this);
            _for_node.statements.visit(this);
        }

        public void visit(repeat_node _repeat_node)
        {
            AddPossibleComments(_repeat_node, true, false);
            _repeat_node.statements.visit(this);
            _repeat_node.expr.visit(this);
        }

        public void visit(while_node _while_node)
        {
            AddPossibleComments(_while_node, true, false);
            _while_node.expr.visit(this);
            _while_node.statements.visit(this);
        }

        public void visit(if_node _if_node)
        {
            AddPossibleComments(_if_node, true, false);
            _if_node.condition.visit(this);
            _if_node.then_body.visit(this);
            if (_if_node.else_body != null)
                _if_node.else_body.visit(this);
        }

        public void visit(ref_type _ref_type)
        {
            AddPossibleComments(_ref_type, true, false);
            _ref_type.pointed_to.visit(this);
        }

        public void visit(diapason _diapason)
        {
            _diapason.left.visit(this);
            _diapason.right.visit(this);
        }

        public void visit(indexers_types _indexers_types)
        {
            foreach (type_definition td in _indexers_types.indexers)
                if (td != null)
                td.visit(this);
        }

        public void visit(array_type _array_type)
        {
            AddPossibleComments(_array_type, true, false);
            if (_array_type.indexers != null)
                _array_type.indexers.visit(this);
            _array_type.elemets_types.visit(this);
        }

        public void visit(label_definitions _label_definitions)
        {
            AddPossibleComments(_label_definitions, true, false);
            _label_definitions.labels.visit(this);
        }

        public void visit(procedure_attribute _procedure_attribute)
        {
            if (_procedure_attribute.source_context != null)
                AddPossibleComments(_procedure_attribute, true, true);
        }

        public void visit(typed_parametres _typed_parametres)
        {
            if (_typed_parametres.param_kind != parametr_kind.none)
                AddPossibleComments(_typed_parametres, true, false);
            _typed_parametres.idents.visit(this);
            _typed_parametres.vars_type.visit(this);
            if (_typed_parametres.inital_value != null)
                _typed_parametres.inital_value.visit(this);
        }

        public void visit(formal_parametres _formal_parametres)
        {
            foreach (typed_parametres tp in _formal_parametres.params_list)
                tp.visit(this);
        }

        public void visit(procedure_attributes_list _procedure_attributes_list)
        {
            foreach (procedure_attribute pa in _procedure_attributes_list.proc_attributes)
                pa.visit(this);
        }

        public void visit(procedure_header _procedure_header)
        {
            AddPossibleComments(_procedure_header, true, false);
            _procedure_header.name.visit(this);
            if (_procedure_header.parametres != null)
                _procedure_header.parametres.visit(this);
            if (_procedure_header.proc_attributes != null)
                _procedure_header.proc_attributes.visit(this);
            if (_procedure_header.where_defs != null)
                _procedure_header.where_defs.visit(this);
        }

        public void visit(function_header _function_header)
        {
            AddPossibleComments(_function_header, true, false);
            _function_header.name.visit(this);
            if (_function_header.parametres != null)
                _function_header.parametres.visit(this);
            _function_header.return_type.visit(this);
            if (_function_header.proc_attributes != null)
                _function_header.proc_attributes.visit(this);
            if (_function_header.where_defs != null)
                _function_header.where_defs.visit(this);
        }

        public void visit(procedure_definition _procedure_definition)
        {
            _procedure_definition.proc_header.visit(this);
            _procedure_definition.proc_body.visit(this);
        }

        public void visit(type_declaration _type_declaration)
        {
            if (_type_declaration.attributes != null)
                _type_declaration.attributes.visit(this);
            _type_declaration.type_name.visit(this);
            _type_declaration.type_def.visit(this);
        }

        public void visit(type_declarations _type_declarations)
        {
            AddPossibleComments(_type_declarations, true, false);
            foreach (type_declaration td in _type_declarations.types_decl)
                td.visit(this);
        }

        public void visit(simple_const_definition _simple_const_definition)
        {
            _simple_const_definition.const_name.visit(this);
            _simple_const_definition.const_value.visit(this);
        }

        public void visit(typed_const_definition _typed_const_definition)
        {
            _typed_const_definition.const_name.visit(this);
            _typed_const_definition.const_type.visit(this);
            _typed_const_definition.const_value.visit(this);
        }

        public void visit(const_definition _const_definition)
        {
            throw new NotImplementedException();
        }

        public void visit(consts_definitions_list _consts_definitions_list)
        {
            AddPossibleComments(_consts_definitions_list, true, false);
            foreach (const_definition cd in _consts_definitions_list.const_defs)
                cd.visit(this);
        }

        public void visit(unit_name _unit_name)
        {
            AddPossibleComments(_unit_name, true, false);
            _unit_name.idunit_name.visit(this);
        }

        public void visit(unit_or_namespace _unit_or_namespace)
        {
            _unit_or_namespace.name.visit(this);
        }

        public void visit(uses_unit_in _uses_unit_in)
        {
            _uses_unit_in.name.visit(this);
            _uses_unit_in.in_file.visit(this);
        }

        public void visit(uses_list _uses_list)
        {
            AddPossibleComments(_uses_list,true,false);
            foreach (unit_or_namespace un in _uses_list.units)
                un.visit(this);
        }

        public void visit(program_body _program_body)
        {
            throw new NotImplementedException();
        }

        public void visit(compilation_unit _compilation_unit)
        {
            throw new NotImplementedException();
        }

        public void visit(unit_module _unit_module)
        {
            _unit_module.unit_name.visit(this);
            if (_unit_module.interface_part != null)
                _unit_module.interface_part.visit(this);
            if (_unit_module.implementation_part != null)
                _unit_module.implementation_part.visit(this);
            if (_unit_module.initialization_part != null)
                _unit_module.initialization_part.visit(this);
            if (_unit_module.finalization_part != null)
                _unit_module.finalization_part.visit(this);
        }

        public void visit(program_module _program_module)
        {
            if (_program_module.program_name != null)
                _program_module.program_name.visit(this);
            if (_program_module.used_units != null)
                _program_module.used_units.visit(this);
            _program_module.program_block.visit(this);
        }

        public void visit(hex_constant _hex_constant)
        {
            AddPossibleComments(_hex_constant, true, true);
        }

        public void visit(get_address _get_address)
        {
            AddPossibleComments(_get_address, true, true);
        }

        public void visit(case_variant _case_variant)
        {
            throw new NotImplementedException();
        }

        public void visit(case_node _case_node)
        {
            throw new NotImplementedException();
        }

        public void visit(method_name _method_name)
        {
            if (_method_name.class_name != null)
                _method_name.class_name.visit(this);
            if (_method_name.explicit_interface_name != null)
                _method_name.explicit_interface_name.visit(this);
            if (_method_name.meth_name != null)
                _method_name.meth_name.visit(this);
        }

        public void visit(dot_node _dot_node)
        {
            _dot_node.left.visit(this);
            _dot_node.right.visit(this);
        }

        public void visit(empty_statement _empty_statement)
        {
            throw new NotImplementedException();
        }

        public void visit(goto_statement _goto_statement)
        {
            _goto_statement.label.visit(this);
        }

        public void visit(labeled_statement _labeled_statement)
        {
            _labeled_statement.label_name.visit(this);
            _labeled_statement.to_statement.visit(this);
        }

        public void visit(with_statement _with_statement)
        {
            _with_statement.do_with.visit(this);
            _with_statement.what_do.visit(this);
        }

        public void visit(method_call _method_call)
        {
            _method_call.dereferencing_value.visit(this);
            if (_method_call.parametres != null)
            _method_call.parametres.visit(this);
        }

        public void visit(pascal_set_constant _pascal_set_constant)
        {
            AddPossibleComments(_pascal_set_constant, true, true);
            if (_pascal_set_constant.values != null)
                _pascal_set_constant.values.visit(this);
        }

        public void visit(array_const _array_const)
        {
            AddPossibleComments(_array_const, true, true);
            _array_const.elements.visit(this);
        }

        public void visit(write_accessor_name _write_accessor_name)
        {
            throw new NotImplementedException();
        }

        public void visit(read_accessor_name _read_accessor_name)
        {
            throw new NotImplementedException();
        }

        public void visit(property_accessors _property_accessors)
        {
            throw new NotImplementedException();
        }

        public void visit(simple_property _simple_property)
        {
            throw new NotImplementedException();
        }

        public void visit(index_property _index_property)
        {
            throw new NotImplementedException();
        }

        public void visit(class_members _class_members)
        {
            throw new NotImplementedException();
        }

        public void visit(access_modifer_node _access_modifer_node)
        {
            throw new NotImplementedException();
        }

        public void visit(class_body _class_body)
        {
            throw new NotImplementedException();
        }

        public void visit(class_definition _class_definition)
        {
            throw new NotImplementedException();
        }

        public void visit(default_indexer_property_node _default_indexer_property_node)
        {
            throw new NotImplementedException();
        }

        public void visit(known_type_definition _known_type_definition)
        {
            throw new NotImplementedException();
        }

        public void visit(set_type_definition _set_type_definition)
        {
            throw new NotImplementedException();
        }

        public void visit(try_statement _try_statement)
        {
            throw new NotImplementedException();
        }

        public void visit(on_exception _on_exception)
        {
            throw new NotImplementedException();
        }

        public void visit(on_exception_list _on_exception_list)
        {
            throw new NotImplementedException();
        }

        public void visit(try_finally_statement _try_finally_statement)
        {
            throw new NotImplementedException();
        }

        public void visit(try_except_statement _try_except_statement)
        {
            throw new NotImplementedException();
        }

        public void visit(record_const_definition _record_const_definition)
        {
            throw new NotImplementedException();
        }

        public void visit(record_const _record_const)
        {
            throw new NotImplementedException();
        }

        public void visit(record_type _record_type)
        {
            throw new NotImplementedException();
        }

        public void visit(enum_type_definition _enum_type_definition)
        {
            throw new NotImplementedException();
        }

        public void visit(char_const _char_const)
        {
            AddPossibleComments(_char_const, true, true);
        }

        public void visit(raise_statement _raise_statement)
        {
            throw new NotImplementedException();
        }

        public void visit(sharp_char_const _sharp_char_const)
        {
            AddPossibleComments(_sharp_char_const, true, true);
        }

        public void visit(literal_const_line _literal_const_line)
        {
            throw new NotImplementedException();
        }

        public void visit(string_num_definition _string_num_definition)
        {
            _string_num_definition.name.visit(this);
            _string_num_definition.num_of_symbols.visit(this);
            AddPossibleComments(_string_num_definition, false, true);
        }

        public void visit(variant _variant)
        {
            throw new NotImplementedException();
        }

        public void visit(variant_list _variant_list)
        {
            throw new NotImplementedException();
        }

        public void visit(variant_type _variant_type)
        {
            throw new NotImplementedException();
        }

        public void visit(variant_types _variant_types)
        {
            throw new NotImplementedException();
        }

        public void visit(variant_record_type _variant_record_type)
        {
            throw new NotImplementedException();
        }

        public void visit(procedure_call _procedure_call)
        {
            throw new NotImplementedException();
        }

        public void visit(class_predefinition _class_predefinition)
        {
            throw new NotImplementedException();
        }

        public void visit(nil_const _nil_const)
        {
            AddPossibleComments(_nil_const, true, true);
        }

        public void visit(file_type_definition _file_type_definition)
        {
            throw new NotImplementedException();
        }

        public void visit(constructor _constructor)
        {
            throw new NotImplementedException();
        }

        public void visit(destructor _destructor)
        {
            throw new NotImplementedException();
        }

        public void visit(inherited_method_call _inherited_method_call)
        {
            throw new NotImplementedException();
        }

        public void visit(typecast_node _typecast_node)
        {
            throw new NotImplementedException();
        }

        public void visit(interface_node _interface_node)
        {
            if (_interface_node.uses_modules != null)
                _interface_node.uses_modules.visit(this);
            if (_interface_node.interface_definitions != null)
                _interface_node.interface_definitions.visit(this);
        }

        public void visit(implementation_node _implementation_node)
        {
            if (_implementation_node.uses_modules != null)
                _implementation_node.uses_modules.visit(this);
            if (_implementation_node.implementation_definitions != null)
                _implementation_node.implementation_definitions.visit(this);
        }

        public void visit(diap_expr _diap_expr)
        {
            throw new NotImplementedException();
        }

        public void visit(block _block)
        {
            if (_block.defs != null)
            _block.defs.visit(this);
            if (_block.program_code != null)
                _block.program_code.visit(this);
        }

        public void visit(proc_block _proc_block)
        {
            throw new NotImplementedException();
        }

        public void visit(array_of_named_type_definition _array_of_named_type_definition)
        {
            throw new NotImplementedException();
        }

        public void visit(array_of_const_type_definition _array_of_const_type_definition)
        {
            throw new NotImplementedException();
        }

        public void visit(literal _literal)
        {
            AddPossibleComments(_literal, true, true);
        }

        public void visit(case_variants _case_variants)
        {
            throw new NotImplementedException();
        }

        public void visit(diapason_expr _diapason_expr)
        {
            _diapason_expr.left.visit(this);
            _diapason_expr.right.visit(this);
        }

        public void visit(var_def_list _var_def_list)
        {
            throw new NotImplementedException();
        }

        public void visit(record_type_parts _record_type_parts)
        {
            throw new NotImplementedException();
        }

        public void visit(property_array_default _property_array_default)
        {
            throw new NotImplementedException();
        }

        public void visit(property_interface _property_interface)
        {
            throw new NotImplementedException();
        }

        public void visit(property_parameter _property_parameter)
        {
            throw new NotImplementedException();
        }

        public void visit(property_parameter_list _property_parameter_list)
        {
            throw new NotImplementedException();
        }

        public void visit(inherited_ident _inherited_ident)
        {
            throw new NotImplementedException();
        }

        public void visit(format_expr _format_expr)
        {
            _format_expr.expr.visit(this);
            _format_expr.format1.visit(this);
            if (_format_expr.format2 != null)
                _format_expr.format2.visit(this);
        }

        public void visit(initfinal_part _initfinal_part)
        {
            throw new NotImplementedException();
        }

        public void visit(token_info _token_info)
        {
            throw new NotImplementedException();
        }

        public void visit(raise_stmt _raise_stmt)
        {
            AddPossibleComments(_raise_stmt, true, false);
            _raise_stmt.expr.visit(this);
        }

        public void visit(op_type_node _op_type_node)
        {
            throw new NotImplementedException();
        }

        public void visit(file_type _file_type)
        {
            AddPossibleComments(_file_type, true, false);
            if (_file_type.file_of_type != null)
            _file_type.file_of_type.visit(this);
        }

        public void visit(known_type_ident _known_type_ident)
        {
            throw new NotImplementedException();
        }

        public void visit(exception_handler _exception_handler)
        {
            throw new NotImplementedException();
        }

        public void visit(exception_ident _exception_ident)
        {
            throw new NotImplementedException();
        }

        public void visit(exception_handler_list _exception_handler_list)
        {
            throw new NotImplementedException();
        }

        public void visit(exception_block _exception_block)
        {
            throw new NotImplementedException();
        }

        public void visit(try_handler _try_handler)
        {
            throw new NotImplementedException();
        }

        public void visit(try_handler_finally _try_handler_finally)
        {
            throw new NotImplementedException();
        }

        public void visit(try_handler_except _try_handler_except)
        {
            throw new NotImplementedException();
        }

        public void visit(try_stmt _try_stmt)
        {
            AddPossibleComments(_try_stmt, true, false);
            _try_stmt.stmt_list.visit(this);
            _try_stmt.handler.visit(this);
        }

        public void visit(inherited_message _inherited_message)
        {
            throw new NotImplementedException();
        }

        public void visit(external_directive _external_directive)
        {
            throw new NotImplementedException();
        }

        public void visit(using_list _using_list)
        {
            throw new NotImplementedException();
        }

        public void visit(oberon_import_module _oberon_import_module)
        {
            throw new NotImplementedException();
        }

        public void visit(oberon_module _oberon_module)
        {
            throw new NotImplementedException();
        }

        public void visit(oberon_ident_with_export_marker _oberon_ident_with_export_marker)
        {
            throw new NotImplementedException();
        }

        public void visit(oberon_exit_stmt _oberon_exit_stmt)
        {
            throw new NotImplementedException();
        }

        public void visit(jump_stmt _jump_stmt)
        {
            throw new NotImplementedException();
        }

        public void visit(oberon_procedure_receiver _oberon_procedure_receiver)
        {
            throw new NotImplementedException();
        }

        public void visit(oberon_procedure_header _oberon_procedure_header)
        {
            throw new NotImplementedException();
        }

        public void visit(oberon_withstmt_guardstat _oberon_withstmt_guardstat)
        {
            throw new NotImplementedException();
        }

        public void visit(oberon_withstmt_guardstat_list _oberon_withstmt_guardstat_list)
        {
            throw new NotImplementedException();
        }

        public void visit(oberon_withstmt _oberon_withstmt)
        {
            throw new NotImplementedException();
        }

        public void visit(loop_stmt _loop_stmt)
        {
            throw new NotImplementedException();
        }

        public void visit(foreach_stmt _foreach_stmt)
        {
            AddPossibleComments(_foreach_stmt, true, false);
            _foreach_stmt.identifier.visit(this);
            if (_foreach_stmt.type_name != null)
                _foreach_stmt.type_name.visit(this);
            _foreach_stmt.in_what.visit(this);
            _foreach_stmt.stmt.visit(this);
        }

        public void visit(addressed_value_funcname _addressed_value_funcname)
        {
            throw new NotImplementedException();
        }

        public void visit(named_type_reference_list _named_type_reference_list)
        {
            foreach (named_type_reference ntr in _named_type_reference_list.types)
                ntr.visit(this);
        }

        public void visit(template_param_list _template_param_list)
        {
            throw new NotImplementedException();
        }

        public void visit(template_type_reference _template_type_reference)
        {
            throw new NotImplementedException();
        }

        public void visit(int64_const _int64_const)
        {
            AddPossibleComments(_int64_const, true, true);
        }

        public void visit(uint64_const _uint64_const)
        {
            AddPossibleComments(_uint64_const, true, true);
        }

        public void visit(new_expr _new_expr)
        {
            AddPossibleComments(_new_expr, true, false);
            _new_expr.type.visit(this);
            if (_new_expr.params_list != null)
                _new_expr.params_list.visit(this);
            if (_new_expr.array_init_expr != null)
                _new_expr.array_init_expr.visit(this);
        }

        public void visit(type_definition_list _type_definition_list)
        {
            throw new NotImplementedException();
        }

        public void visit(where_definition _where_definition)
        {
            throw new NotImplementedException();
        }

        public void visit(where_definition_list _where_definition_list)
        {
            throw new NotImplementedException();
        }

        public void visit(sizeof_operator _sizeof_operator)
        {
            AddPossibleComments(_sizeof_operator, true, true);
            if (_sizeof_operator.type_def != null)
                _sizeof_operator.type_def.visit(this);
            if (_sizeof_operator.expr != null)
                _sizeof_operator.expr.visit(this);
        }

        public void visit(typeof_operator _typeof_operator)
        {
            AddPossibleComments(_typeof_operator, true, true);
            _typeof_operator.type_name.visit(this);
        }

        public void visit(compiler_directive _compiler_directive)
        {
            throw new NotImplementedException();
        }

        public void visit(operator_name_ident _operator_name_ident)
        {
            throw new NotImplementedException();
        }

        public void visit(var_statement _var_statement)
        {
            AddPossibleComments(_var_statement, true, false);
        }

        public void visit(question_colon_expression _question_colon_expression)
        {
            _question_colon_expression.condition.visit(this);
            _question_colon_expression.ret_if_true.visit(this);
            _question_colon_expression.ret_if_false.visit(this);
        }

        public void visit(expression_as_statement _expression_as_statement)
        {
            throw new NotImplementedException();
        }

        public void visit(c_scalar_type _c_scalar_type)
        {
            throw new NotImplementedException();
        }

        public void visit(c_module _c_module)
        {
            throw new NotImplementedException();
        }

        public void visit(declarations_as_statement _declarations_as_statement)
        {
            throw new NotImplementedException();
        }

        public void visit(array_size _array_size)
        {
            throw new NotImplementedException();
        }

        public void visit(enumerator _enumerator)
        {
            if (_enumerator.name != null)
                _enumerator.name.visit(this);
            _enumerator.value.visit(this);
        }

        public void visit(enumerator_list _enumerator_list)
        {
            foreach (enumerator en in _enumerator_list.enumerators)
                en.visit(this);
        }

        public void visit(c_for_cycle _c_for_cycle)
        {
            throw new NotImplementedException();
        }

        public void visit(switch_stmt _switch_stmt)
        {
            throw new NotImplementedException();
        }

        public void visit(type_definition_attr_list _type_definition_attr_list)
        {
            throw new NotImplementedException();
        }

        public void visit(type_definition_attr _type_definition_attr)
        {
            throw new NotImplementedException();
        }

        public void visit(lock_stmt _lock_stmt)
        {
            throw new NotImplementedException();
        }

        public void visit(compiler_directive_list _compiler_directive_list)
        {
            throw new NotImplementedException();
        }

        public void visit(compiler_directive_if _compiler_directive_if)
        {
            throw new NotImplementedException();
        }

        public void visit(documentation_comment_list _documentation_comment_list)
        {
            throw new NotImplementedException();
        }

        public void visit(documentation_comment_tag _documentation_comment_tag)
        {
            throw new NotImplementedException();
        }

        public void visit(documentation_comment_tag_param _documentation_comment_tag_param)
        {
            throw new NotImplementedException();
        }

        public void visit(documentation_comment_section _documentation_comment_section)
        {
            throw new NotImplementedException();
        }

        public void visit(token_taginfo _token_taginfo)
        {
            throw new NotImplementedException();
        }

        public void visit(declaration_specificator _declaration_specificator)
        {
            throw new NotImplementedException();
        }

        public void visit(ident_with_templateparams _ident_with_templateparams)
        {
            throw new NotImplementedException();
        }

        public void visit(template_type_name _template_type_name)
        {
            throw new NotImplementedException();
        }

        public void visit(default_operator _default_operator)
        {
            AddPossibleComments(_default_operator, true, true);
            _default_operator.type_name.visit(this);
        }

        public void visit(bracket_expr _bracket_expr)
        {
            AddPossibleComments(_bracket_expr, true, true);
            _bracket_expr.expr.visit(this);
        }

        public void visit(attribute _attribute)
        {
            throw new NotImplementedException();
        }

        public void visit(simple_attribute_list _simple_attribute_list)
        {
            throw new NotImplementedException();
        }

        public void visit(attribute_list _attribute_list)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}