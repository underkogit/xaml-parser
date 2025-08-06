use roxmltree::{Document, Node};
use std::ffi::{CStr, CString};
use std::os::raw::c_char;

#[repr(C)]
pub struct XamlAttribute {
    key: *mut c_char,
    value: *mut c_char,
}

#[repr(C)]
pub struct XamlElement {
    name: *mut c_char,
    namespace: *mut c_char,
    attributes: *mut XamlAttribute,
    attributes_len: usize,
    children: *mut *mut XamlElement,
    children_len: usize,
    text_content: *mut c_char,
}

// NativeXamlParser::ParseXaml - парсинг XAML документа
#[unsafe(no_mangle)]
pub extern "C" fn parse_xaml(xml: *const c_char, result: *mut *mut XamlElement) -> i32 {
    if xml.is_null() || result.is_null() {
        return -1;
    }

    let xml_str = match unsafe { CStr::from_ptr(xml) }.to_str() {
        Ok(s) => s,
        Err(_) => return -2,
    };

    let doc = match Document::parse(xml_str) {
        Ok(d) => d,
        Err(_) => return -3,
    };

    let root_element = convert_node_to_xaml_element(doc.root_element());
    let boxed = Box::new(root_element);
    unsafe { *result = Box::into_raw(boxed) };
    0
}

// NativeXamlParser::FreeXamlElement - освобождение памяти
#[unsafe(no_mangle)]
pub extern "C" fn free_xaml_element(element: *mut XamlElement) -> i32 {
    if element.is_null() {
        return -1;
    }

    unsafe {
        free_xaml_element_internal(Box::from_raw(element));
    }
    0
}

fn convert_node_to_xaml_element(node: Node) -> XamlElement {
    let name = CString::new(node.tag_name().name()).unwrap().into_raw();
    let namespace = if let Some(ns) = node.tag_name().namespace() {
        CString::new(ns).unwrap().into_raw()
    } else {
        std::ptr::null_mut()
    };

    let attrs: Vec<XamlAttribute> = node.attributes().map(|attr| {
        XamlAttribute {
            key: CString::new(attr.name()).unwrap().into_raw(),
            value: CString::new(attr.value()).unwrap().into_raw(),
        }
    }).collect();

    let children: Vec<*mut XamlElement> = node.children()
        .filter(|n| n.is_element())
        .map(|child| Box::into_raw(Box::new(convert_node_to_xaml_element(child))))
        .collect();

    let text_content = if let Some(text) = node.text() {
        CString::new(text).unwrap().into_raw()
    } else {
        std::ptr::null_mut()
    };

    let attributes_ptr = if attrs.is_empty() {
        std::ptr::null_mut()
    } else {
        let boxed_attrs = attrs.into_boxed_slice();
        Box::into_raw(boxed_attrs) as *mut XamlAttribute
    };

    let children_ptr = if children.is_empty() {
        std::ptr::null_mut()
    } else {
        let boxed_children = children.into_boxed_slice();
        Box::into_raw(boxed_children) as *mut *mut XamlElement
    };

    XamlElement {
        name,
        namespace,
        attributes: attributes_ptr,
        attributes_len: node.attributes().count(),
        children: children_ptr,
        children_len: node.children().filter(|n| n.is_element()).count(),
        text_content,
    }
}

fn free_xaml_element_internal(element: Box<XamlElement>) {
    unsafe {
        if !element.name.is_null() {
            let _ = CString::from_raw(element.name);
        }
        if !element.namespace.is_null() {
            let _ = CString::from_raw(element.namespace);
        }
        if !element.text_content.is_null() {
            let _ = CString::from_raw(element.text_content);
        }

        if !element.attributes.is_null() {
            let attrs = Box::from_raw(std::slice::from_raw_parts_mut(
                element.attributes,
                element.attributes_len
            ));
            for attr in attrs.iter() {
                if !attr.key.is_null() {
                    let _ = CString::from_raw(attr.key);
                }
                if !attr.value.is_null() {
                    let _ = CString::from_raw(attr.value);
                }
            }
        }

        if !element.children.is_null() {
            let children = Box::from_raw(std::slice::from_raw_parts_mut(
                element.children,
                element.children_len
            ));
            for &child_ptr in children.iter() {
                if !child_ptr.is_null() {
                    free_xaml_element_internal(Box::from_raw(child_ptr));
                }
            }
        }
    }
}