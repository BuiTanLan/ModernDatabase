:begin
CREATE CONSTRAINT ON (node:`UNIQUE IMPORT LABEL`) ASSERT (node.`UNIQUE IMPORT ID`) IS UNIQUE;
:commit
:begin
UNWIND [{_id:13, properties:{Status:"Pending", PaymentIntentId:"asd", Total:48.12, BuyerEmail:"phandangkhoa999@gmail.com", Subtotal:45.26, uuid:"1", OrderDate:"2021-06-01T18:40:32.142+0700"}}, {_id:14, properties:{Status:"Pending", PaymentIntentId:"asd", Total:80.12, BuyerEmail:"phandangkhoa999@gmail.com", Subtotal:70.26, uuid:"2", OrderDate:"2021-06-01T18:40:32.142+0700"}}] AS row
CREATE (n:`UNIQUE IMPORT LABEL`{`UNIQUE IMPORT ID`: row._id}) SET n += row.properties SET n:ORDER;
UNWIND [{_id:0, properties:{ProductName:"Core Purple Boots", ProductItemId:"60ba6a410d59ab50c4c9617a", Price:199.99, PictureUrl:"images/products/boot-core1.png", uuid:"4ff8e7d1-b28f-4f84-ac47-4feee3662faf"}}, {_id:2, properties:{ProductName:"Redis Red Boots", ProductItemId:"60c2534c235b0c4763590b21", Price:250.0, uuid:"5e5f73f0-d093-4587-ba3c-a0d3f892610a"}}, {_id:3, properties:{ProductName:"Green React Gloves", ProductItemId:"60c253b3235b0c4763590b22", Price:14.0, uuid:"5bfa901c-3074-4620-a7c1-29d2563a7148"}}, {_id:4, properties:{ProductName:"Purple React Gloves", ProductItemId:"60c2544c235b0c4763590b23", Price:16.0, uuid:"e526fdaf-9cf6-421d-af41-904e8fbaac2e"}}, {_id:7, properties:{ProductName:"Green Code Gloves", ProductItemId:"60c25574235b0c4763590b24", Price:15.0, uuid:"ac186ab0-9f01-4c7c-8722-d7c5dd86b34b"}}, {_id:8, properties:{ProductName:"Blue Code Gloves", ProductItemId:"60c2590d235b0c4763590b25", Price:18.0, uuid:"88992b84-e9b3-4642-83bf-4f2bd24a72e4"}}, {_id:9, properties:{ProductName:"Purple React Woolen Hat", ProductItemId:"60c2597b235b0c4763590b26", Price:15.0, uuid:"122de808-ebd4-41b5-89c4-9c69123da4f6"}}, {_id:10, properties:{ProductName:"Green React Woolen Hat", ProductItemId:"60c25994235b0c4763590b27", Price:8.0, price:10, uuid:"4c7ee7af-cb79-4ccd-bb5d-9473e12fa540"}}, {_id:15, properties:{ProductName:"Core Board Speed Rush 3", ProductItemId:"60b7a6392ddd114afc4dc072", Price:45.26, PictureUrl:"images/products/sb-core1.png", uuid:"3"}}, {_id:16, properties:{ProductName:"Typescript Entry Board", ProductItemId:"60c25ab2235b0c4763590b2a", Price:120.0, uuid:"241531f4-e007-4f9c-b5ca-3f9df8547c02"}}, {_id:17, properties:{ProductItemId:"60c25a2f235b0c4763590b29", ProductName:"Core Blue Hat", Price:10.0, uuid:"17b2d993-2d26-4490-ba38-7a890444b34e"}}, {_id:18, properties:{ProductName:"React Board Super Whizzy Fast", ProductItemId:"60c25b96235b0c4763590b2b", Price:250.0, uuid:"bdc406d1-9d90-4a9a-afd7-46c2b8bbc6b0"}}, {_id:19, properties:{ProductName:"Net Core Super Board", ProductItemId:"60c25c76235b0c4763590b2c", Price:300.0, uuid:"dabe1db8-6b63-4c2c-a58b-7ffc1cbbb20a"}}, {_id:20, properties:{ProductName:"Angular Speedster Board 2000", ProductItemId:"60c25cfe235b0c4763590b2d", Price:200.0, uuid:"fe8ed1f0-c84b-4039-9df7-4815183a2a4d"}}, {_id:21, properties:{ProductName:"Green Angular Board 3000", ProductItemId:"60c25d2d235b0c4763590b2e", Price:150.0, uuid:"3499b7a2-6e59-42f1-b02c-ad1c44379276"}}, {_id:22, properties:{ProductName:"Angular Purple Boots", ProductItemId:"60c25e41235b0c4763590b2f", Price:150.0, uuid:"962bb627-1755-46e2-b525-b4987f2a9523"}}, {_id:23, properties:{ProductName:"Angular Blue Boots", ProductItemId:"60c25e63235b0c4763590b30", Price:180.0, uuid:"085f1e69-eaf1-4f9b-b66e-402ace254746"}}] AS row
CREATE (n:`UNIQUE IMPORT LABEL`{`UNIQUE IMPORT ID`: row._id}) SET n += row.properties SET n:PRODUCT;
UNWIND [{_id:5, properties:{DeliveryTime:"1-2 Days", Description:"Fastest delivery time", Price:10, ShortName:"UPS1", Id:1}}, {_id:6, properties:{DeliveryTime:"2-5 Days", Description:"Get it within 5 days", Price:5, ShortName:"UPS2", Id:2}}, {_id:11, properties:{DeliveryTime:"5-10 Days", Description:"Slower but cheap", Price:2, ShortName:"UPS3", Id:3}}, {_id:12, properties:{DeliveryTime:"1-2 Weeks", Description:"Free! You get what you pay for", Price:0, ShortName:"FREE", Id:4}}] AS row
CREATE (n:`UNIQUE IMPORT LABEL`{`UNIQUE IMPORT ID`: row._id}) SET n += row.properties SET n:DELIVERYMETHOD;
UNWIND [{_id:1, properties:{ZipCode:"7000", State:"asd", FirstName:"Khoa", BuyerEmail:"phandangkhoa999@gmail.com", Street:"THD", LastName:"Phan Dang", City:"hcm", uuid:"cd4da089-7dda-401c-a681-901cd295b7fe"}}, {_id:24, properties:{State:"NY", FirstName:"Bob", ZipCode:"90210", BuyerEmail:"bob@test.com", Street:"10 Thw Street", LastName:"Bobbity", City:"New York", uuid:"3b5f1d8a-c616-4f43-9516-ab2439fff744"}}] AS row
CREATE (n:`UNIQUE IMPORT LABEL`{`UNIQUE IMPORT ID`: row._id}) SET n += row.properties SET n:USER;
:commit
:begin
UNWIND [{start: {_id:1}, end: {_id:13}, properties:{}}, {start: {_id:1}, end: {_id:14}, properties:{}}] AS row
MATCH (start:`UNIQUE IMPORT LABEL`{`UNIQUE IMPORT ID`: row.start._id})
MATCH (end:`UNIQUE IMPORT LABEL`{`UNIQUE IMPORT ID`: row.end._id})
CREATE (start)-[r:ARRANGE]->(end) SET r += row.properties;
UNWIND [{start: {_id:1}, end: {_id:15}, properties:{}}, {start: {_id:1}, end: {_id:0}, properties:{}}] AS row
MATCH (start:`UNIQUE IMPORT LABEL`{`UNIQUE IMPORT ID`: row.start._id})
MATCH (end:`UNIQUE IMPORT LABEL`{`UNIQUE IMPORT ID`: row.end._id})
CREATE (start)-[r:BUY]->(end) SET r += row.properties;
UNWIND [{start: {_id:13}, end: {_id:15}, properties:{Quantity:1}}, {start: {_id:13}, end: {_id:0}, properties:{Quantity:1}}, {start: {_id:14}, end: {_id:15}, properties:{Quantity:2}}, {start: {_id:14}, end: {_id:0}, properties:{Quantity:1}}] AS row
MATCH (start:`UNIQUE IMPORT LABEL`{`UNIQUE IMPORT ID`: row.start._id})
MATCH (end:`UNIQUE IMPORT LABEL`{`UNIQUE IMPORT ID`: row.end._id})
CREATE (start)-[r:CONTAIN]->(end) SET r += row.properties;
UNWIND [{start: {_id:13}, end: {_id:12}, properties:{}}, {start: {_id:14}, end: {_id:11}, properties:{}}] AS row
MATCH (start:`UNIQUE IMPORT LABEL`{`UNIQUE IMPORT ID`: row.start._id})
MATCH (end:`UNIQUE IMPORT LABEL`{`UNIQUE IMPORT ID`: row.end._id})
CREATE (start)-[r:USE]->(end) SET r += row.properties;
:commit
:begin
MATCH (n:`UNIQUE IMPORT LABEL`)  WITH n LIMIT 20000 REMOVE n:`UNIQUE IMPORT LABEL` REMOVE n.`UNIQUE IMPORT ID`;
:commit
:begin
DROP CONSTRAINT ON (node:`UNIQUE IMPORT LABEL`) ASSERT (node.`UNIQUE IMPORT ID`) IS UNIQUE;
:commit
