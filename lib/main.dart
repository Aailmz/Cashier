import 'package:flutter/material.dart';
import 'package:qr_code_scanner/qr_code_scanner.dart';
import 'package:http/http.dart' as http;
import 'dart:convert';

void main() {
  runApp(const MainApp());
}

class MainApp extends StatelessWidget {
  const MainApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      home: Scaffold(
        appBar: AppBar(title: const Text('QR Code Scanner')),
        body: const QRViewExample(),
      ),
    );
  }
}

class QRViewExample extends StatefulWidget {
  const QRViewExample({Key? key}) : super(key: key);

  @override
  _QRViewExampleState createState() => _QRViewExampleState();
}

class _QRViewExampleState extends State<QRViewExample> {
  final GlobalKey qrKey = GlobalKey(debugLabel: 'QR');
  QRViewController? controller;
  String? qrText;

  @override
  void reassemble() {
    super.reassemble();
    if (controller != null) {
      controller!.pauseCamera();
      controller!.resumeCamera();
    }
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      children: <Widget>[
        Expanded(
          flex: 5,
          child: QRView(
            key: qrKey,
            onQRViewCreated: _onQRViewCreated,
            overlay: QrScannerOverlayShape(
              borderColor: Colors.red,
              borderRadius: 10,
              borderLength: 30,
              borderWidth: 10,
              cutOutSize: 300,
            ),
          ),
        ),
        Expanded(
          flex: 1,
          child: Center(
            child: Text(qrText ?? 'Scan a code'),
          ),
        ),
        ElevatedButton(
          onPressed: () {
            controller?.resumeCamera();
          },
          child: const Text('Scan Again'),
        ),
        ElevatedButton(
          onPressed: () {
            _sendScannedData(qrText);
          },
          child: const Text('Send Data to Server'),
        ),
      ],
    );
  }

  void _onQRViewCreated(QRViewController controller) {
    this.controller = controller;
    controller.scannedDataStream.listen((scanData) async {
      setState(() {
        qrText = scanData.code;
      });

      // Pause the camera after scanning
      controller.pauseCamera();

      // Now send the scanned data to the server
      await _sendScannedData(qrText);
    });
  }

  Future<void> _sendScannedData(String? qrJson) async {
    if (qrJson == null) return;

    try {
      // Decode the JSON data from the scanned QR code
      final Map<String, dynamic> dataReceived = jsonDecode(qrJson);
      
      // Ensure the data contains the necessary fields
      final String name = dataReceived["Name"] ?? "Unknown";
      final String code = dataReceived["Code"] ?? "Unknown";
      final String price = dataReceived["Price"]?.toString() ?? "0";
      final String stock = dataReceived["Stock"]?.toString() ?? "0";

      // Create a JSON object to send to the server
      final Map<String, String> dataToSend = {
        "Name": name,
        "Code": code,
        "Price": price,
        "Stock": stock,
      };
      final String jsonData = jsonEncode(dataToSend);

      final response = await http.post(
        Uri.parse('http://192.168.1.179:8000/'),
        headers: {'Content-Type': 'application/json'},
        body: jsonData,
      );

      if (response.statusCode == 200) {
        print('Data sent successfully');
      } else {
        print('Failed to send data: ${response.statusCode}');
      }
    } catch (e) {
      print('Error: $e');
    }
  }

  @override
  void dispose() {
    controller?.dispose();
    super.dispose();
  }
}
