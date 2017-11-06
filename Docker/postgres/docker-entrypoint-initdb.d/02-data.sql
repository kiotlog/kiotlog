--
-- PostgreSQL database dump
--

\connect kiotlog

-- Dumped from database version 9.6.4
-- Dumped by pg_dump version 10.0

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;

SET search_path = public, pg_catalog;

--
-- Data for Name: conversions; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY conversions (id, fun) FROM stdin;
0c57d818-f542-45ff-b710-eae777124aed	float_to_int16
a31dbdaa-1d56-4bc3-8ee6-facb2a9ada55	float_to_uint16
5b635746-c7aa-4604-8f8d-0066fe84a5a2	id
\.


--
-- Data for Name: devices; Type: TABLE DATA; Schema: public; Owner: kiotlog_writers
--

COPY devices (id, device, meta, auth, frame) FROM stdin;
98b32d84-a522-4a80-84bc-2bdabe65c0b5	18A9E5	{"name": "mkrfox-toolbox", "description": "MKRFOX1200 @ ToolBox"}	{"basic": {"token": "afa1d877-f4f7-4b38-b559-48fd00d9ae1b"}}	{"bigendian": true, "bitfields": false}
86d36df8-6468-4ce6-a54a-e13fdb9a8c57	18B8D6	{"name": "mkrfox-roaming", "description": "MKRFOX1200 @ Roaming"}	{"basic": {"token": "fb2e2658-c6cd-498d-91e6-37ad92bbe89b"}}	{"bigendian": true, "bitfields": false}
\.


--
-- Data for Name: sensor_types; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY sensor_types (id, name, meta, type) FROM stdin;
eab4c8d2-7d00-46f3-9492-92525bb3c67d	Generic_Temperature	\N	temperature
2e9e5330-5bde-4f5b-9c1f-52c96d73a05e	DHT11_Temperature	{"max": 50, "min": 0}	temperature
6d2504fd-6a10-4295-aa9c-0b4d615941e2	DHT11_Humidity	{"max": 90, "min": 20}	humidity
ae2f0f47-1e6d-4bbd-908b-d204857ba1bd	DHT22_Temperature	{"max": 120, "min": -40}	temperature
d9508fcb-9061-41b6-a112-6849dd7c5739	DHT22_Humidity	{"max": 100, "min": 0}	humidity
8b4f41b0-48ef-4004-b30c-8560b52cd3b7	Generic	{}	generic
\.


--
-- Data for Name: sensors; Type: TABLE DATA; Schema: public; Owner: kiotlog_writers
--

COPY sensors (id, meta, fmt, conversion, sensor_type, device_id) FROM stdin;
e03995dc-9691-43f3-abcb-fe617060e2ef	{"name": "temperature"}	{"index": 1, "fmt_chr": "h"}	0c57d818-f542-45ff-b710-eae777124aed	2e9e5330-5bde-4f5b-9c1f-52c96d73a05e	98b32d84-a522-4a80-84bc-2bdabe65c0b5
796c234f-4023-48b8-ba27-e5f31c3bd578	{"name": "humidity"}	{"index": 2, "fmt_chr": "H"}	a31dbdaa-1d56-4bc3-8ee6-facb2a9ada55	6d2504fd-6a10-4295-aa9c-0b4d615941e2	98b32d84-a522-4a80-84bc-2bdabe65c0b5
9ebc099a-62bd-47cb-a71d-d9d615af12fc	{"name": "temperature_internal"}	{"index": 3, "fmt_chr": "h"}	0c57d818-f542-45ff-b710-eae777124aed	eab4c8d2-7d00-46f3-9492-92525bb3c67d	98b32d84-a522-4a80-84bc-2bdabe65c0b5
ddb85793-a9b8-4836-862c-ab1980ebfe9a	{"name": "last_status"}	{"index": 4, "fmt_chr": "B"}	5b635746-c7aa-4604-8f8d-0066fe84a5a2	8b4f41b0-48ef-4004-b30c-8560b52cd3b7	98b32d84-a522-4a80-84bc-2bdabe65c0b5
43df5a85-d579-4de2-aac8-399258488cab	{"name": "status"}	{"index": 0, "fmt_chr": "B"}	5b635746-c7aa-4604-8f8d-0066fe84a5a2	8b4f41b0-48ef-4004-b30c-8560b52cd3b7	98b32d84-a522-4a80-84bc-2bdabe65c0b5
8991ba0d-6263-4a98-ba39-b39b527ba09a	{"name": "temperature"}	{"index": 1, "fmt_chr": "h"}	0c57d818-f542-45ff-b710-eae777124aed	ae2f0f47-1e6d-4bbd-908b-d204857ba1bd	86d36df8-6468-4ce6-a54a-e13fdb9a8c57
acceb97a-abf7-458e-96ac-800ec3c2722c	{"name": "humidity"}	{"index": 2, "fmt_chr": "H"}	a31dbdaa-1d56-4bc3-8ee6-facb2a9ada55	d9508fcb-9061-41b6-a112-6849dd7c5739	86d36df8-6468-4ce6-a54a-e13fdb9a8c57
5e20265f-e176-4f75-8c81-ff219a9984e7	{"name": "temperature_internal"}	{"index": 3, "fmt_chr": "h"}	0c57d818-f542-45ff-b710-eae777124aed	8b4f41b0-48ef-4004-b30c-8560b52cd3b7	86d36df8-6468-4ce6-a54a-e13fdb9a8c57
cb710293-fa09-4fc6-8e16-db8b8ad5ff2c	{"name": "status"}	{"index": 0, "fmt_chr": "B"}	5b635746-c7aa-4604-8f8d-0066fe84a5a2	8b4f41b0-48ef-4004-b30c-8560b52cd3b7	86d36df8-6468-4ce6-a54a-e13fdb9a8c57
c319187d-4d91-4d67-b6e6-f83f13e774c0	{"name": "last_status"}	{"index": 4, "fmt_chr": "B"}	5b635746-c7aa-4604-8f8d-0066fe84a5a2	8b4f41b0-48ef-4004-b30c-8560b52cd3b7	86d36df8-6468-4ce6-a54a-e13fdb9a8c57
\.


--
-- PostgreSQL database dump complete
--

